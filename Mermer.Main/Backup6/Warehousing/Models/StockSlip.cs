// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Models.StockSlip
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Transactions.Models;
using System;

#nullable disable
namespace Mermer.Warehousing.Models;

public class StockSlip : StockTransaction<StockSlipLine>
{
  private StockSlipType _slipType;

  public virtual StockSlipType SlipType
  {
    get => this._slipType;
    set
    {
      this.SetProperty<StockSlipType>(ref this._slipType, value, nameof (SlipType), "Type", "IsStockIncome", "IsPriceEditable");
    }
  }

  public override string Type => this.SlipType.ToString();

  public override bool IsStockIncome
  {
    get
    {
      switch (this.SlipType)
      {
        case StockSlipType.StockOpening:
        case StockSlipType.RevisionExceed:
          return true;
        case StockSlipType.StockSpoilage:
        case StockSlipType.StockUsage:
        case StockSlipType.RevisionDeficit:
          return false;
        default:
          throw new ArgumentOutOfRangeException("SlipType");
      }
    }
  }

  public bool IsPriceEditable => this.SlipType == StockSlipType.StockOpening;
}
