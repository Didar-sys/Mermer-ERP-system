using System;

namespace Mermer.Ui.Pc.DTOs
{
    public class InvoiceDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; }
        public string PartnerId { get; set; }
        public string Description { get; set; }
    }
}