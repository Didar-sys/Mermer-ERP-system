// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Reporting.Models.AggregatedReport
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.StockManagement.Models;

#nullable disable
namespace Payhas.Binyat.Reporting.Models;

public class AggregatedReport
{
  public FundsBalanceAggregated FundsReport { get; set; }

  public StockBalanceAggregated StocksReport { get; set; }

  public PartnerBalanceAggregated PartnersReport { get; set; }
}
