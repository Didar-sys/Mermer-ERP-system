// Decompiled with JetBrains decompiler
// Type: Mermer.Finance.DailyRegistery.Models.DailyFundsRegisteryInfo
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using System;

#nullable disable
namespace Mermer.Finance.DailyRegistery.Models;

public class DailyFundsRegisteryInfo : DailyFundsRegistery
{
  public Decimal? Computed { get; set; }

  public Decimal? Difference
  {
    get
    {
      if (!this.Computed.HasValue)
        return new Decimal?();
      Decimal actionTotal = this.ActionTotal;
      Decimal? computed = this.Computed;
      return !computed.HasValue ? new Decimal?() : new Decimal?(actionTotal - computed.GetValueOrDefault());
    }
  }

  public bool IsExceed => this.Difference.HasValue && this.Difference.Value > 0M;

  public bool IsDeficit => this.Difference.HasValue && this.Difference.Value < 0M;
}
