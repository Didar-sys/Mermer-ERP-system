// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Transactions.Models.Validators.StockUnitConvertionValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Transactions.Models.Validators;

public class StockUnitConvertionValidator : AbstractValidator<StockUnitConvertion>
{
  public StockUnitConvertionValidator()
  {
    this.RuleFor<string>((Expression<Func<StockUnitConvertion, string>>) (x => x.Id)).NotEmpty<StockUnitConvertion, string>();
    this.RuleFor<string>((Expression<Func<StockUnitConvertion, string>>) (x => x.StockId)).NotEmpty<StockUnitConvertion, string>();
    this.RuleFor<string>((Expression<Func<StockUnitConvertion, string>>) (x => x.UnitId)).NotEmpty<StockUnitConvertion, string>();
    this.RuleFor<Decimal>((Expression<Func<StockUnitConvertion, Decimal>>) (x => x.Multiplier)).GreaterThan<StockUnitConvertion, Decimal>(0M);
    this.RuleFor<Decimal>((Expression<Func<StockUnitConvertion, Decimal>>) (x => x.Divider)).GreaterThan<StockUnitConvertion, Decimal>(0M);
  }
}
