// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.Mappers.BillReportLineMapper
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Mermer.Commerce.Models;
using Mermer.Ui.Pc.Reports.Helpers;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public class BillReportLineMapper(NameHelper nameHelper) : 
  FundsTransactionReportLineMapper<BillLine, BillReportLine>(nameHelper)
{
}
