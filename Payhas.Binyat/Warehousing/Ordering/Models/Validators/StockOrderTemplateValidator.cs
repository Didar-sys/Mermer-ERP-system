// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Ordering.Models.Validators.StockOrderTemplateValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.Common.Models.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Warehousing.Ordering.Models.Validators;

public class StockOrderTemplateValidator : AbstractModelValidator<StockOrderTemplate>
{
  public StockOrderTemplateValidator(StockOrderTemplateLineValidator lineValidator)
  {
    this.RuleFor<string>((Expression<Func<StockOrderTemplate, string>>) (x => x.Name)).NotEmpty<StockOrderTemplate, string>();
    ((IRuleBuilder<StockOrderTemplate, IEnumerable<StockOrderTemplateLine>>) this.RuleFor<ObservableCollection<StockOrderTemplateLine>>((Expression<Func<StockOrderTemplate, ObservableCollection<StockOrderTemplateLine>>>) (x => x.Lines)).Must<StockOrderTemplate, ObservableCollection<StockOrderTemplateLine>>((Func<ObservableCollection<StockOrderTemplateLine>, bool>) (x => x != null && x.Any<StockOrderTemplateLine>())).WithLocalizationMessageKey<StockOrderTemplate, ObservableCollection<StockOrderTemplateLine>>("{PropertyName} can not be empty")).SetCollectionValidator<StockOrderTemplate, StockOrderTemplateLine>((IValidator<StockOrderTemplateLine>) lineValidator);
  }
}
