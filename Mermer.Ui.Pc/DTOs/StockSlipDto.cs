using System;

namespace Mermer.Ui.Pc.DTOs
{
    public class StockSlipDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string SlipType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsCompleted { get; set; }
        public decimal DisplayTotal { get; set; }
        public string Description { get; set; }
    }
}