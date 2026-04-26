// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.Validators.StockAlternativeValueValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.StockManagement.Models.Validators;

public class StockAlternativeValueValidator : AbstractValidator<StockAlternativeLine>
{
  public StockAlternativeValueValidator()
  {
    this.RuleFor<string>((Expression<Func<StockAlternativeLine, string>>) (x => x.StockId)).NotEmpty<StockAlternativeLine, string>();
  }
}
