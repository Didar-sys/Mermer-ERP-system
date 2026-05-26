// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Models.Validators.StockTransferValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.StockManagement.Services;
using Mermer.Transactions.Models;
using Mermer.Transactions.Models.Validators;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Warehousing.Models.Validators;

public class StockTransferValidator : StockTransactionValidator<StockTransfer, StockTransferLine>
{
  public StockTransferValidator(
    IValidator<StockTransferLine> lineValidator,
    IValidator<StockUnitConvertion> stockUnitConvertionValidator,
    IValidator<CurrencyConvertion> currencyConvertionValidator,
    IValidator<StockTransactionOverhead> overheadValidator,
    IStockBalancesRepository stockBalancesRepository)
    : base(lineValidator, stockUnitConvertionValidator, currencyConvertionValidator, overheadValidator, stockBalancesRepository)
  {
    this.RuleFor<string>((Expression<Func<StockTransfer, string>>) (x => x.DestinationWarehouseId)).NotEmpty<StockTransfer, string>().Must<StockTransfer, string>((Func<StockTransfer, string, bool>) ((x, val) => val != x.WarehouseId)).WithLocalizationMessageKey<StockTransfer, string>("Source & Destination warehouses should be different");
  }
}
