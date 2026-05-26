// Decompiled with JetBrains decompiler
// Type: Mermer.Common.Models.Validators.ValidationLanguageManager
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation.Resources;
using Mermer.Common.Services;
using System.Globalization;

#nullable disable
namespace Mermer.Common.Models.Validators;

public class ValidationLanguageManager : LanguageManager
{
  private readonly ILocalizationService _localization;

  public ValidationLanguageManager(ILocalizationService localization)
  {
    this._localization = localization;
  }

  public override string GetString(string key, CultureInfo culture = null)
  {
    string entryKey = base.GetString(key, culture);
    if (string.IsNullOrEmpty(entryKey))
      entryKey = key;
    return this._localization.GetText(entryKey);
  }
}
