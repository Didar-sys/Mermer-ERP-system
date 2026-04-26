// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Common.Models.Validators.ValidatorOptionsSetter
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Autofac;
using FluentValidation;
using FluentValidation.Resources;
using Payhas.Binyat.Common.Services;
using System;
using System.Linq.Expressions;
using System.Reflection;

#nullable disable
namespace Payhas.Binyat.Common.Models.Validators;

public class ValidatorOptionsSetter : IStartable
{
  private readonly ILocalizationService _localization;
  private readonly ILanguageManager _languageManager;

  public ValidatorOptionsSetter(ILocalizationService localization, ILanguageManager languageManager)
  {
    this._localization = localization;
    this._languageManager = languageManager;
  }

  public void Start()
  {
    ValidatorOptions.LanguageManager = this._languageManager;
    ValidatorOptions.DisplayNameResolver = (Func<Type, MemberInfo, LambdaExpression, string>) ((type, info, arg3) => this._localization.GetText(info.Name));
  }
}
