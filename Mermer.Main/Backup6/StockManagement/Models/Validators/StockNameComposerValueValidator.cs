// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.Validators.StockNameComposerValueValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.StockManagement.Models.Validators;

public class StockNameComposerValueValidator : AbstractValidator<StockNameComposerValue>
{
  public StockNameComposerValueValidator()
  {
    this.RuleFor<string>((Expression<Func<StockNameComposerValue, string>>) (x => x.ShortName)).NotEmpty<StockNameComposerValue, string>();
    this.RuleFor<string>((Expression<Func<StockNameComposerValue, string>>) (x => x.Name)).NotEmpty<StockNameComposerValue, string>();
  }
}
