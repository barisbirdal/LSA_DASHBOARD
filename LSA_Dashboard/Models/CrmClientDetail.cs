using System;
using System.Collections.Generic;

namespace LSA_Dashboard.Models;

public partial class CrmClientDetail
{
    public int OrganizationId { get; set; }

    public int? SectorId { get; set; }

    public int? SegmentId { get; set; }

    public string? Country { get; set; }

    public string? Region { get; set; }

    public int? AccountOwnerUserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<CrmContract> CrmContracts { get; set; } = new List<CrmContract>();

    public virtual CrmSector? Sector { get; set; }

    public virtual CrmSegment? Segment { get; set; }
}
