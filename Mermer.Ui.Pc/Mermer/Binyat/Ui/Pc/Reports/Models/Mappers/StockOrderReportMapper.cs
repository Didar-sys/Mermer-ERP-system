// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.Mappers.StockOrderReportMapper
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Mermer.Ui.Pc.Reports.Helpers;
using Mermer.Warehousing.Ordering.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public class StockOrderReportMapper
{
  private readonly NameHelper _nameHelper;
  private readonly StockOrderReportLineMapper _lineMapper;

  public StockOrderReportMapper(NameHelper nameHelper, StockOrderReportLineMapper lineMapper)
  {
    this._nameHelper = nameHelper;
    this._lineMapper = lineMapper;
  }

  public async Task<StockOrderReport> Map(StockOrder source, string localizedType)
  {
    List<StockOrderReportLine> lines = new List<StockOrderReportLine>();
    for (int i = 0; i < source.Lines.Count; ++i)
    {
      StockOrderReportLine stockOrderReportLine = await this._lineMapper.Map(source.Lines[i]);
      stockOrderReportLine.RowNo = i + 1;
      lines.Add(stockOrderReportLine);
    }
    StockOrderReport stockOrderReport1 = new StockOrderReport();
    stockOrderReport1.Code = source.Code;
    stockOrderReport1.Date = source.Date;
    stockOrderReport1.Type = localizedType;
    StockOrderReport stockOrderReport2 = stockOrderReport1;
    stockOrderReport2.Warehouse = await this._nameHelper.GetWarehouseName(source.WarehouseId);
    stockOrderReport1.UserName = source.UserName;
    stockOrderReport1.IsCompleted = source.IsCompleted;
    stockOrderReport1.IsDisabled = source.IsDisabled;
    stockOrderReport1.Lines = (IEnumerable<StockOrderReportLine>) lines;
    StockOrderReport stockOrderReport3 = stockOrderReport1;
    stockOrderReport2 = (StockOrderReport) null;
    stockOrderReport1 = (StockOrderReport) null;
    StockOrderReport stockOrderReport4 = stockOrderReport3;
    lines = (List<StockOrderReportLine>) null;
    return stockOrderReport4;
  }
}
