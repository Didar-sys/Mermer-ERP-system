// Decompiled with JetBrains decompiler
// Type: Mermer.Reporting.Models.AggregatedReport
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.CRM.Models;
using Mermer.FundsManagement.Models;
using Mermer.StockManagement.Models;

#nullable disable
namespace Mermer.Reporting.Models;

public class AggregatedReport
{
  public FundsBalanceAggregated FundsReport { get; set; }

  public StockBalanceAggregated StocksReport { get; set; }

  public PartnerBalanceAggregated PartnersReport { get; set; }
}
