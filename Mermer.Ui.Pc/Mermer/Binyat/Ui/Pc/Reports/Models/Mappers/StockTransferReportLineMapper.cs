using Mermer.Ui.Pc.Reports.Helpers;
using Mermer.Warehousing.Models;
using System.Threading.Tasks;

namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public class StockTransferReportLineMapper(NameHelper nameHelper) :
    StockTransactionReportLineMapper<StockTransferLine, StockTransferReportLine>(nameHelper)
{
    public override async Task<StockTransferReportLine> Map(StockTransferLine source)
    {
        StockTransferReportLine destination = await base.Map(source);

        destination.ReceivedQuantity = source.ReceivedQuantity;
        destination.ReceivedUnit = await NameHelper.GetStockUnitName(source.StockId, source.UnitId);
        destination.ReceivedTotal = source.DisplayReceivedTotal;

        return destination;
    }
}