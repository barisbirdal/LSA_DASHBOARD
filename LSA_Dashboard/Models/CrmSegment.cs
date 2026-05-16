using System;
using System.Collections.Generic;

namespace LSA_Dashboard.Models;

public partial class CrmSegment
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<CrmClientDetail> CrmClientDetails { get; set; } = new List<CrmClientDetail>();
}
