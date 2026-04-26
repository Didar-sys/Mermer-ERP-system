// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.StockManagement.Models.Validators.StockNameComposerValidator
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
namespace Payhas.Binyat.StockManagement.Models.Validators;

public class StockNameComposerValidator : AbstractModelValidator<StockNameComposer>
{
  public StockNameComposerValidator(IValidator<StockNameComposerValue> valueValidator)
  {
    this.RuleFor<string>((Expression<Func<StockNameComposer, string>>) (x => x.Name)).NotEmpty<StockNameComposer, string>();
    ((IRuleBuilder<StockNameComposer, IEnumerable<StockNameComposerValue>>) this.RuleFor<ObservableCollection<StockNameComposerValue>>((Expression<Func<StockNameComposer, ObservableCollection<StockNameComposerValue>>>) (x => x.Values)).Must<StockNameComposer, ObservableCollection<StockNameComposerValue>>((Func<ObservableCollection<StockNameComposerValue>, bool>) (x => x != null && x.Any<StockNameComposerValue>())).WithLocalizationMessageKey<StockNameComposer, ObservableCollection<StockNameComposerValue>>("{PropertyName} must be specified")).SetCollectionValidator<StockNameComposer, StockNameComposerValue>(valueValidator);
  }
}
