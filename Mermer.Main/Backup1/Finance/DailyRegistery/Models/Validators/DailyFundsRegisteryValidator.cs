// Decompiled with JetBrains decompiler
// Type: Mermer.Finance.DailyRegistery.Models.Validators.DailyFundsRegisteryValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;

#nullable disable
namespace Mermer.Finance.DailyRegistery.Models.Validators;

public class DailyFundsRegisteryValidator(
  IValidator<DailyFundsRegisteryLine> lineValidator,
  IValidator<CurrencyConvertion> currencyConvertionValidator) : 
  FundsTransactionValidator<DailyFundsRegistery, DailyFundsRegisteryLine>(lineValidator, currencyConvertionValidator)
{
}
