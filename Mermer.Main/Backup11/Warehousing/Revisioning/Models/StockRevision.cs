// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Revisioning.Models.StockRevision
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Transactions.Models;
using System;

#nullable disable
namespace Mermer.Warehousing.Revisioning.Models;

public class StockRevision : TransactionModel
{
  private DateTime? _finishDate;
  private string _warehouseId;
  private string _exceedSlipId;
  private string _deficitSlipId;

  public override string Type => nameof (StockRevision);

  public DateTime? FinishDate
  {
    get => this._finishDate;
    set => this.SetProperty<DateTime?>(ref this._finishDate, value, nameof (FinishDate));
  }

  public string WarehouseId
  {
    get => this._warehouseId;
    set => this.SetProperty<string>(ref this._warehouseId, value, nameof (WarehouseId));
  }

  public string ExceedSlipId
  {
    get => this._exceedSlipId;
    set => this.SetProperty<string>(ref this._exceedSlipId, value, nameof (ExceedSlipId));
  }

  public string DeficitSlipId
  {
    get => this._deficitSlipId;
    set => this.SetProperty<string>(ref this._deficitSlipId, value, nameof (DeficitSlipId));
  }
}
