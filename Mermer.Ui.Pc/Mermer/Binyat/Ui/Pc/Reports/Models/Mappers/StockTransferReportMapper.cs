// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.Mappers.StockTransferReportMapper
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Mermer.Ui.Pc.Reports.Helpers;
using Mermer.Warehousing.Models;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public class StockTransferReportMapper(
  NameHelper nameHelper,
  StockTransferReportLineMapper lineMapper) : 
  StockTransactionReportMapper<StockTransfer, StockTransferLine, StockTransferReport, StockTransferReportLine>(nameHelper, (TransactionReportLineMapper<StockTransferLine, StockTransferReportLine>) lineMapper)
{
  public override async Task<StockTransferReport> Map(StockTransfer source, string localizedType)
  {
    StockTransferReportMapper transferReportMapper = this;
    // ISSUE: reference to a compiler-generated method
    StockTransferReport destination = await transferReportMapper.\u003C\u003En__0(source, localizedType);
    StockTransferReport stockTransferReport = destination;
    stockTransferReport.DestinationWarehouse = await transferReportMapper.NameHelper.GetWarehouseName(source.DestinationWarehouseId);
    stockTransferReport = (StockTransferReport) null;
    StockTransferReport stockTransferReport1 = destination;
    destination = (StockTransferReport) null;
    return stockTransferReport1;
  }
}
