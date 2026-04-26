// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Finance.Models.FundsTransfer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Transactions.Models;
using Payhas.Data;
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Finance.Models;

public class FundsTransfer : FundsTransaction<FundsTransferLine>
{
  private string _destinationDepositoryId;

  public virtual string DestinationDepositoryId
  {
    get => this._destinationDepositoryId;
    set
    {
      this.SetProperty<string>(ref this._destinationDepositoryId, value, nameof (DestinationDepositoryId));
    }
  }

  public override bool IsFundsIncome => false;

  public override string Type => nameof (FundsTransfer);

  public virtual bool IsConflicted
  {
    get
    {
      WatchedObservableCollection<FundsTransferLine> lines = this.Lines;
      return lines != null && lines.Any<FundsTransferLine>((Func<FundsTransferLine, bool>) (x => x.Amount != x.ReceivedAmount));
    }
  }

  public Decimal ActionReceivedTotal
  {
    get
    {
      WatchedObservableCollection<FundsTransferLine> lines = this.Lines;
      return lines == null ? 0M : lines.Sum<FundsTransferLine>((Func<FundsTransferLine, Decimal>) (x => x.ActionReceivedTotal));
    }
  }

  protected override void LinePropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    base.LinePropertyChanged(sender, e);
    this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsConflicted));
    this.RaisePropertyChanged<Decimal>((Expression<Func<Decimal>>) (() => this.ActionReceivedTotal));
  }
}
