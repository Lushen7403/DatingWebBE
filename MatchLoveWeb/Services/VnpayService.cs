using MatchLoveWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web;

public interface IVnpayService
{
    string CreatePaymentUrl(long amount, string orderInfo, string returnUrl, string txnRef, string? ipAddress = null);
    bool ValidateSignature(IQueryCollection query);
    bool ValidateSignature(IFormCollection form);
}

public class VnpayService : IVnpayService
{
    private readonly VnpayOptions _opt;

    public VnpayService(IOptions<VnpayOptions> opt) => _opt = opt.Value;

    public string CreatePaymentUrl(long amount, string orderInfo, string returnUrl, string txnRef, string? ipAddress = null)
    {
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
            ipAddress = "127.0.0.1";

        // Chuẩn bị tham số
        var vnpParams = new SortedDictionary<string, string>
        {
            {"vnp_Version", _opt.Version},
            {"vnp_Command", _opt.Command},
            {"vnp_TmnCode", _opt.TmnCode},
            {"vnp_Amount", (amount * 100).ToString(CultureInfo.InvariantCulture)},
            {"vnp_CurrCode", _opt.CurrCode},
            {"vnp_TxnRef", txnRef},
            {"vnp_OrderInfo", orderInfo},
            {"vnp_OrderType", "other"},
            {"vnp_ReturnUrl", returnUrl},
            {"vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")},
            {"vnp_Locale", _opt.Locale},
            {"vnp_IpAddr", ipAddress}
        };

        // Build và encode tham số theo thứ tự, dùng Uri.EscapeDataString
        var builder = new StringBuilder();
        foreach (var kv in vnpParams)
        {
            if (builder.Length > 0)
                builder.Append('&');
            builder.Append(Uri.EscapeDataString(kv.Key))
                   .Append('=')
                   .Append(Uri.EscapeDataString(kv.Value));
        }

        var dataToHash = builder.ToString();

        // Tính HMAC SHA512 trên chuỗi đã encode
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_opt.HashSecret.Trim()));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
        var secureHash = BitConverter.ToString(hashBytes)
                                 .Replace("-", string.Empty)
                                 .ToLowerInvariant();

        // Tạo URL cuối cùng
        var paymentUrl = new StringBuilder(_opt.BaseUrl)
            .Append('?').Append(dataToHash)
            .Append("&vnp_SecureHashType=SHA512")
            .Append("&vnp_SecureHash=").Append(secureHash);

        return paymentUrl.ToString();
    }

    public bool ValidateSignature(IQueryCollection query)
    {
        var receivedHash = query["vnp_SecureHash"].ToString();
        var vnpParams = new SortedDictionary<string, string>();
        foreach (var key in query.Keys)
        {
            if (key == "vnp_SecureHash" || key == "vnp_SecureHashType") continue;
            vnpParams[key] = query[key];
        }

        // Build and encode data string same as Create
        var builder = new StringBuilder();
        foreach (var kv in vnpParams)
        {
            if (builder.Length > 0)
                builder.Append('&');
            builder.Append(Uri.EscapeDataString(kv.Key))
                   .Append('=')
                   .Append(Uri.EscapeDataString(kv.Value));
        }
        var dataToHash = builder.ToString();

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_opt.HashSecret.Trim()));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
        var calculatedHash = BitConverter.ToString(hashBytes)
                                  .Replace("-", string.Empty).ToLowerInvariant();

        return calculatedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
    }

    public bool ValidateSignature(IFormCollection form)
    {
        var receivedHash = form["vnp_SecureHash"].ToString();
        var vnpParams = new SortedDictionary<string, string>();
        foreach (var key in form.Keys)
        {
            if (key == "vnp_SecureHash" || key == "vnp_SecureHashType") continue;
            vnpParams[key] = form[key];
        }

        var builder = new StringBuilder();
        foreach (var kv in vnpParams)
        {
            if (builder.Length > 0)
                builder.Append('&');
            builder.Append(Uri.EscapeDataString(kv.Key))
                   .Append('=')
                   .Append(Uri.EscapeDataString(kv.Value));
        }
        var dataToHash = builder.ToString();

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_opt.HashSecret.Trim()));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
        var calculatedHash = BitConverter.ToString(hashBytes)
                                  .Replace("-", string.Empty).ToLowerInvariant();

        return calculatedHash.Equals(receivedHash, StringComparison.OrdinalIgnoreCase);
    }
}
