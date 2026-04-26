// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Models.Validators.StockSlipValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Models.Validators;

#nullable disable
namespace Payhas.Binyat.Warehousing.Models.Validators;

public class StockSlipValidator(
  IValidator<StockSlipLine> lineValidator,
  IValidator<StockUnitConvertion> stockUnitConvertionValidator,
  IValidator<CurrencyConvertion> currencyConvertionValidator,
  IValidator<StockTransactionOverhead> overheadValidator,
  IStockBalancesRepository stockBalancesRepository) : 
  StockTransactionValidator<StockSlip, StockSlipLine>(lineValidator, stockUnitConvertionValidator, currencyConvertionValidator, overheadValidator, stockBalancesRepository)
{
}
