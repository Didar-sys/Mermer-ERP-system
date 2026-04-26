// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.Mappers.StockTransferReportLineMapper
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Payhas.Binyat.Ui.Pc.Reports.Helpers;
using Payhas.Binyat.Warehousing.Models;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models.Mappers;

public class StockTransferReportLineMapper(NameHelper nameHelper) : 
  StockTransactionReportLineMapper<StockTransferLine, StockTransferReportLine>(nameHelper)
{
  public override async Task<StockTransferReportLine> Map(StockTransferLine source)
  {
    StockTransferReportLineMapper reportLineMapper = this;
    // ISSUE: reference to a compiler-generated method
    StockTransferReportLine destination = await reportLineMapper.\u003C\u003En__0(source);
    destination.ReceivedQuantity = source.ReceivedQuantity;
    StockTransferReportLine transferReportLine = destination;
    transferReportLine.ReceivedUnit = await reportLineMapper.NameHelper.GetStockUnitName(source.StockId, source.UnitId);
    transferReportLine = (StockTransferReportLine) null;
    destination.ReceivedTotal = source.DisplayReceivedTotal;
    StockTransferReportLine transferReportLine1 = destination;
    destination = (StockTransferReportLine) null;
    return transferReportLine1;
  }
}
