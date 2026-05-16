using System;
using System.Collections.Generic;

namespace LSA_Dashboard.Models;

public partial class CrmInvoice
{
    public int Id { get; set; }

    public int? OrganizationId { get; set; }

    public int? ContractId { get; set; }

    public DateOnly? BillingPeriod { get; set; }

    public decimal? FixedFee { get; set; }

    public decimal? UsageBasedFee { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public virtual CrmContract? Contract { get; set; }
}
