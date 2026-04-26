// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Finance.Models.FundsSlip
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Transactions.Models;
using System;

#nullable disable
namespace Payhas.Binyat.Finance.Models;

public class FundsSlip : FundsTransaction<FundsSlipLine>
{
  private FundsSlipType _slipType;

  public virtual FundsSlipType SlipType
  {
    get => this._slipType;
    set => this.SetProperty<FundsSlipType>(ref this._slipType, value, nameof (SlipType), "Type");
  }

  public override string Type => this.SlipType.ToString();

  public override bool IsFundsIncome
  {
    get
    {
      switch (this.SlipType)
      {
        case FundsSlipType.FundsOpening:
        case FundsSlipType.FundsRevisionExceed:
          return true;
        case FundsSlipType.FundsRevisionDeficit:
          return false;
        default:
          throw new ArgumentOutOfRangeException("SlipType");
      }
    }
  }
}
