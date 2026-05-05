// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.Mappers.StockTransactionReportLineMapper`2
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Mermer.Transactions.Models;
using Mermer.Ui.Pc.Reports.Helpers;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public abstract class StockTransactionReportLineMapper<TLine, TReportLine> : 
  TransactionReportLineMapper<TLine, TReportLine>
  where TLine : StockTransactionLine
  where TReportLine : StockTransactionReportLine
{
  public StockTransactionReportLineMapper(NameHelper nameHelper)
    : base(nameHelper)
  {
  }

  public override async Task<TReportLine> Map(TLine source)
  {
    StockTransactionReportLineMapper<TLine, TReportLine> reportLineMapper = this;
    // ISSUE: reference to a compiler-generated method
    TReportLine destination = await reportLineMapper.\u003C\u003En__0(source);
    TReportLine reportLine = destination;
    reportLine.Stock = await reportLineMapper.NameHelper.GetStockName(source.StockId);
    reportLine = default (TReportLine);
    destination.Quantity = source.Quantity;
    reportLine = destination;
    reportLine.Unit = await reportLineMapper.NameHelper.GetStockUnitName(source.StockId, source.UnitId);
    reportLine = default (TReportLine);
    destination.Price = source.Price;
    TReportLine reportLine1 = destination;
    destination = default (TReportLine);
    return reportLine1;
  }
}
