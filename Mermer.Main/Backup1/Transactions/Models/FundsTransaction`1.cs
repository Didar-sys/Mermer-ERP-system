// Decompiled with JetBrains decompiler
// Type: Mermer.Transactions.Models.FundsTransaction`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

#nullable disable
namespace Mermer.Transactions.Models;

public abstract class FundsTransaction<T> : Transaction<T> where T : FundsTransactionLine
{
  private string _depositoryId;

  public virtual string DepositoryId
  {
    get => this._depositoryId;
    set => this.SetProperty<string>(ref this._depositoryId, value, nameof (DepositoryId));
  }

  public abstract bool IsFundsIncome { get; }
}
