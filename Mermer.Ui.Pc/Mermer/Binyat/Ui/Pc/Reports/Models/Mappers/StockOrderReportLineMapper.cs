// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.Mappers.StockOrderReportLineMapper
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Mermer.Ui.Pc.Reports.Helpers;
using Mermer.Warehousing.Ordering.Models;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public class StockOrderReportLineMapper
{
  private readonly NameHelper _nameHelper;

  public StockOrderReportLineMapper(NameHelper nameHelper) => this._nameHelper = nameHelper;

  public async Task<StockOrderReportLine> Map(StockOrderLine source)
  {
    StockOrderReportLine stockOrderReportLine1 = new StockOrderReportLine();
    StockOrderReportLine stockOrderReportLine2 = stockOrderReportLine1;
    stockOrderReportLine2.Stock = await this._nameHelper.GetStockName(source.StockId);
    stockOrderReportLine1.Quantity = source.Quantity;
    StockOrderReportLine stockOrderReportLine3 = stockOrderReportLine1;
    stockOrderReportLine3.Unit = await this._nameHelper.GetStockUnitName(source.StockId, source.UnitId);
    StockOrderReportLine stockOrderReportLine = stockOrderReportLine1;
    stockOrderReportLine2 = (StockOrderReportLine) null;
    stockOrderReportLine3 = (StockOrderReportLine) null;
    stockOrderReportLine1 = (StockOrderReportLine) null;
    return stockOrderReportLine;
  }
}
