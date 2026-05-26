// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Models.Validators.StockSlipValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.StockManagement.Services;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;

#nullable disable
namespace Mermer.Warehousing.Models.Validators;

public class StockSlipValidator(
  IValidator<StockSlipLine> lineValidator,
  IValidator<StockUnitConvertion> stockUnitConvertionValidator,
  IValidator<CurrencyConvertion> currencyConvertionValidator,
  IValidator<StockTransactionOverhead> overheadValidator,
  IStockBalancesRepository stockBalancesRepository) : 
  StockTransactionValidator<StockSlip, StockSlipLine>(lineValidator, stockUnitConvertionValidator, currencyConvertionValidator, overheadValidator, stockBalancesRepository)
{
}
