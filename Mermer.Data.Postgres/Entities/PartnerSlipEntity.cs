using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mermer.Data.Postgres.Entities;

[Table("partner_slips")]
public class PartnerSlipEntity
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("code")]
    public string Code { get; set; } = null!;

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("slip_type")]
    public string SlipType { get; set; } = null!;

    [Column("office_id")]
    public Guid? OfficeId { get; set; }

    [Column("is_disabled")]
    public bool IsDisabled { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public ICollection<PartnerSlipLineEntity> Lines { get; set; } = new List<PartnerSlipLineEntity>();
}