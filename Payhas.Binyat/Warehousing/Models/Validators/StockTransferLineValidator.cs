// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Models.Validators.StockTransferLineValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.Transactions.Models.Validators;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Warehousing.Models.Validators;

public class StockTransferLineValidator : StockTransactionLineValidator<StockTransferLine>
{
  public StockTransferLineValidator()
  {
    this.RuleFor<string>((Expression<Func<StockTransferLine, string>>) (x => x.ReceivedId)).NotEmpty<StockTransferLine, string>();
    this.RuleFor<Decimal>((Expression<Func<StockTransferLine, Decimal>>) (x => x.ReceivedQuantity)).GreaterThanOrEqualTo<StockTransferLine, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<StockTransferLine, string>>) (x => x.ReceivedUnitId)).NotEmpty<StockTransferLine, string>();
  }
}
