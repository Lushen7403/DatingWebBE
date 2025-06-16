using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MatchLoveWeb.Models;

namespace MatchLoveWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly DatingWebContext _context;
        private readonly IVnpayService _vnpayService;
        private readonly string _returnUrl;

        public PaymentController(
            DatingWebContext context,
            IVnpayService vnpayService,
            IOptions<PaymentSettings> paySettings)
        {
            _context = context;
            _vnpayService = vnpayService;
            _returnUrl = paySettings.Value.ReturnUrl;
        }

        // DTOs
        public class CreatePaymentRequest
        {
            public int AccountId { get; set; }
            public int PackageId { get; set; }
            public int? VoucherId { get; set; }
            public decimal AmountBefore { get; set; }
            public decimal DiscountAmount { get; set; }
        }

        public class PaymentSettings
        {
            public string ReturnUrl { get; set; } = string.Empty;
        }

        /// <summary>
        /// Tạo bản ghi Payment và trả về URL VNPAY để redirect
        /// </summary>
        [HttpPost("create-url")]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] CreatePaymentRequest req)
        {
            // Tính toán số tiền thực thanh toán
            decimal amountAfter = req.AmountBefore - req.DiscountAmount;

            // Tạo bản ghi Payment với trạng thái false (Pending)
            var payment = new Payment
            {
                AccountId = req.AccountId,
                PackageId = req.PackageId,
                VoucherId = req.VoucherId,
                AmountBefore = req.AmountBefore,
                DiscountAmount = req.DiscountAmount,
                AmountAfter = amountAfter,
                Status = false,
                CreatedAt = DateTime.Now
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Gọi VNPAY tạo URL
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            string vnpUrl = _vnpayService.CreatePaymentUrl(
                amount: (long)amountAfter,
                orderInfo: $"Payment{payment.Id}",
                returnUrl: _returnUrl,
                txnRef: payment.Id.ToString(),
                ipAddress: clientIp
            );

            return Ok(new { paymentUrl = vnpUrl });
        }

        /// <summary>
        /// Xử lý redirect từ VNPAY
        /// </summary>
        [HttpGet("return")]
        public async Task<IActionResult> VnpayReturn()
        {
            var query = HttpContext.Request.Query;
            if (!int.TryParse(query["vnp_TxnRef"], out int paymentId))
                return BadRequest("Invalid transaction reference");

            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return NotFound();

            string rspCode = query["vnp_ResponseCode"];
            string txnStatus = query["vnp_TransactionStatus"];
            string bankCode = query["vnp_BankCode"];
            string bankTranNo = query["vnp_BankTranNo"];
            string secureHash = query["vnp_SecureHash"];
            string payDateStr = query["vnp_PayDate"];

            // Validate signature
            bool isValid = _vnpayService.ValidateSignature(HttpContext.Request.Query);
            bool success = isValid && rspCode == "00" && txnStatus == "00";
            // Cập nhật trạng thái
            payment.Status = success;

            // Lưu chi tiết VNPAY
            payment.VnpTxnRef = query["vnp_TxnRef"]; ;
            payment.VnpResponseCode = rspCode;
            payment.VnpTransactionStatus = txnStatus;
            payment.VnpBankCode = bankCode;
            payment.VnpBankTranNo = bankTranNo;
            payment.VnpSecureHash = secureHash;
            if (DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime payDate))
                payment.VnpPayDate = payDate;
            payment.UpdatedAt = DateTime.Now;
            if (success)
            {
                var package = await _context.RechargePackages
                    .FirstOrDefaultAsync(p => p.Id == payment.PackageId && p.IsActivate);
                if (package != null)
                {
                    var account = await _context.Accounts.FindAsync(payment.AccountId);
                    if (account != null)
                    {
                        account.DiamondCount += package.DiamondCount;
                        account.UpdatedAt = DateTime.Now;
                        _context.Accounts.Update(account);
                    }
                }
            }

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            return Ok(new { payment.Status, payment.Id });
        }

        /// <summary>
        /// Xử lý IPN từ VNPAY (server-to-server)
        /// </summary>
        [HttpPost("ipn")]
        public async Task<IActionResult> VnpayIpn()
        {
            var form = HttpContext.Request.Form;
            if (!int.TryParse(form["vnp_TxnRef"], out int paymentId))
                return BadRequest();

            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return NotFound();

            string rspCode = form["vnp_ResponseCode"];
            string txnStatus = form["vnp_TransactionStatus"];
            string bankCode = form["vnp_BankCode"];
            string bankTranNo = form["vnp_BankTranNo"];
            string secureHash = form["vnp_SecureHash"];
            string payDateStr = form["vnp_PayDate"];

            bool isValid = _vnpayService.ValidateSignature(HttpContext.Request.Form);
            if (!isValid)
                return BadRequest(new { RspCode = "97", Message = "Invalid signature" });

            bool success = rspCode == "00" && txnStatus == "00";
            payment.Status = success;

            payment.VnpTxnRef = form["vnp_TxnRef"];
            payment.VnpResponseCode = rspCode;
            payment.VnpTransactionStatus = txnStatus;
            payment.VnpBankCode = bankCode;
            payment.VnpBankTranNo = bankTranNo;
            payment.VnpSecureHash = secureHash;
            if (DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out DateTime payDate))
                payment.VnpPayDate = payDate;
            payment.UpdatedAt = DateTime.Now;

            if (success)
            {
                var package = await _context.RechargePackages
                    .FirstOrDefaultAsync(p => p.Id == payment.PackageId && p.IsActivate);
                if (package != null)
                {
                    var account = await _context.Accounts.FindAsync(payment.AccountId);
                    if (account != null)
                    {
                        account.DiamondCount += package.DiamondCount;
                        account.UpdatedAt = DateTime.Now;
                        _context.Accounts.Update(account);
                    }
                }
            }

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }
    }
}

