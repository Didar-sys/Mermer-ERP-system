using Mermer.Transactions.Models;
using Mermer.Ui.Pc.Reports.Helpers;
using System.Threading.Tasks;

namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public abstract class StockTransactionReportLineMapper<TLine, TReportLine>(NameHelper nameHelper) :
    TransactionReportLineMapper<TLine, TReportLine>(nameHelper)
    where TLine : StockTransactionLine
    where TReportLine : StockTransactionReportLine
{
    public override async Task<TReportLine> Map(TLine source)
    {
        TReportLine destination = await base.Map(source);

        destination.Stock = await NameHelper.GetStockName(source.StockId);
        destination.Quantity = source.Quantity;
        destination.Unit = await NameHelper.GetStockUnitName(source.StockId, source.UnitId);
        destination.Price = source.Price;

        return destination;
    }
}