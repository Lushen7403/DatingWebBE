using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int PackageId { get; set; }

    public int? VoucherId { get; set; }

    public decimal AmountBefore { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal AmountAfter { get; set; }

    public bool Status { get; set; }

    public string? VnpTxnRef { get; set; }
    public string? VnpResponseCode { get; set; }
    public string? VnpTransactionStatus { get; set; }
    public string? VnpBankCode { get; set; }
    public string? VnpBankTranNo { get; set; }
    public DateTime? VnpPayDate { get; set; }
    public string? VnpSecureHash { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual RechargePackage Package { get; set; } = null!;

    public virtual Voucher? Voucher { get; set; }
}
