// Decompiled with JetBrains decompiler
// Type: Mermer.StockManagement.Models.Validators.StockAlternativeValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Common.Models.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.StockManagement.Models.Validators;

public class StockAlternativeValidator : AbstractModelValidator<StockAlternative>
{
  public StockAlternativeValidator(IValidator<StockAlternativeLine> valueValidator)
  {
    this.RuleFor<string>((Expression<Func<StockAlternative, string>>) (x => x.Name)).NotEmpty<StockAlternative, string>();
    ((IRuleBuilder<StockAlternative, IEnumerable<StockAlternativeLine>>) this.RuleFor<ObservableCollection<StockAlternativeLine>>((Expression<Func<StockAlternative, ObservableCollection<StockAlternativeLine>>>) (x => x.Lines)).Must<StockAlternative, ObservableCollection<StockAlternativeLine>>((Func<ObservableCollection<StockAlternativeLine>, bool>) (x => x != null && x.Any<StockAlternativeLine>())).WithLocalizationMessageKey<StockAlternative, ObservableCollection<StockAlternativeLine>>("{PropertyName} must be specified")).SetCollectionValidator<StockAlternative, StockAlternativeLine>(valueValidator);
  }
}
