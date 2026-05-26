// Decompiled with JetBrains decompiler
// Type: Mermer.Finance.Models.Validators.FundsTransferLineValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Transactions.Models.Validators;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Finance.Models.Validators;

public class FundsTransferLineValidator : FundsTransactionLineValidator<FundsTransferLine>
{
  public FundsTransferLineValidator()
  {
    this.RuleFor<Decimal>((Expression<Func<FundsTransferLine, Decimal>>) (x => x.ReceivedAmount)).GreaterThanOrEqualTo<FundsTransferLine, Decimal>(0M);
  }
}
