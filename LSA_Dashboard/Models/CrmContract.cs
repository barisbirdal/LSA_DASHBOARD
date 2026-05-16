using System;
using System.Collections.Generic;

namespace LSA_Dashboard.Models;

public partial class CrmContract
{
    public int Id { get; set; }

    public int? OrganizationId { get; set; }

    public string? ContractType { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool? AutoRenew { get; set; }

    public string? PaymentMethod { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<CrmInvoice> CrmInvoices { get; set; } = new List<CrmInvoice>();

    public virtual CrmClientDetail? Organization { get; set; }
}
