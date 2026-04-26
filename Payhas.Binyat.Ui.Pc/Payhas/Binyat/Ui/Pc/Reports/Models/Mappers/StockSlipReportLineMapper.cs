// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.Mappers.StockSlipReportLineMapper
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Payhas.Binyat.Ui.Pc.Reports.Helpers;
using Payhas.Binyat.Warehousing.Models;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models.Mappers;

public class StockSlipReportLineMapper(NameHelper nameHelper) : 
  StockTransactionReportLineMapper<StockSlipLine, StockSlipReportLine>(nameHelper)
{
}
