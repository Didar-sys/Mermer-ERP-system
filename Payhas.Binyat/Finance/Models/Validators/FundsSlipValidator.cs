// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Finance.Models.Validators.FundsSlipValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Models.Validators;

#nullable disable
namespace Payhas.Binyat.Finance.Models.Validators;

public class FundsSlipValidator(
  IValidator<FundsSlipLine> lineValidator,
  IValidator<CurrencyConvertion> currencyConvertionValidator) : 
  FundsTransactionValidator<FundsSlip, FundsSlipLine>(lineValidator, currencyConvertionValidator)
{
}
