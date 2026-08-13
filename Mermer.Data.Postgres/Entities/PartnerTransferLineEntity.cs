using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mermer.Data.Postgres.Entities;

[Table("partner_transfer_lines")]
public class PartnerTransferLineEntity
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("partner_transfer_id")]
    public Guid PartnerTransferId { get; set; }

    [Column("office_id")]
    public Guid? OfficeId { get; set; }

    [Column("partner_id")]
    public Guid? PartnerId { get; set; }

    [Column("debit_amount")]
    public decimal DebitAmount { get; set; }

    [Column("debit_currency_id")]
    public Guid? DebitCurrencyId { get; set; }

    [Column("credit_amount")]
    public decimal CreditAmount { get; set; }

    [Column("credit_currency_id")]
    public Guid? CreditCurrencyId { get; set; }

    [JsonIgnore]
    public PartnerTransferEntity PartnerTransfer { get; set; } = null!;
}