// Decompiled with JetBrains decompiler
// Type: Mermer.Finance.Models.Validators.FundsTransferValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Finance.Models.Validators;

public class FundsTransferValidator : FundsTransactionValidator<FundsTransfer, FundsTransferLine>
{
  public FundsTransferValidator(
    IValidator<FundsTransferLine> lineValidator,
    IValidator<CurrencyConvertion> currencyConvertionValidator)
    : base(lineValidator, currencyConvertionValidator)
  {
    this.RuleFor<string>((Expression<Func<FundsTransfer, string>>) (x => x.DestinationDepositoryId)).NotEmpty<FundsTransfer, string>().Must<FundsTransfer, string>((Func<FundsTransfer, string, bool>) ((x, val) => val != x.DepositoryId)).WithLocalizationMessageKey<FundsTransfer, string>("Source & Destination should be different");
    this.RuleFor<bool>((Expression<Func<FundsTransfer, bool>>) (x => x.IsCompleted)).NotEqual<FundsTransfer, bool>(true).When<FundsTransfer, bool>((Func<FundsTransfer, bool>) (x => x.IsConflicted)).WithLocalizationMessageKey<FundsTransfer, bool>("Tranfer can not be completed while conflicted");
  }
}
