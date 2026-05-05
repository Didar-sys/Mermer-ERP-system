// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Ordering.Models.Validators.StockOrderTemplateLineValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Warehousing.Ordering.Models.Validators;

public class StockOrderTemplateLineValidator : AbstractValidator<StockOrderTemplateLine>
{
  public StockOrderTemplateLineValidator()
  {
    this.RuleFor<string>((Expression<Func<StockOrderTemplateLine, string>>) (x => x.Id)).NotEmpty<StockOrderTemplateLine, string>();
    this.RuleFor<string>((Expression<Func<StockOrderTemplateLine, string>>) (x => x.StockId)).NotEmpty<StockOrderTemplateLine, string>();
  }
}
