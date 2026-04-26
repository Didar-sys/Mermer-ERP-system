// Decompiled with JetBrains decompiler
// Type: FluentValidation.ValidatorExtender
// Assembly: Payhas, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E04E48D-05E5-40EA-900A-CFBEE8B9F238
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.dll

using FluentValidation.Internal;
using FluentValidation.Resources;
using FluentValidation.Results;
using System;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace FluentValidation;

public static class ValidatorExtender
{
  public static IRuleBuilderOptions<T, TProperty> WithLocalizationMessageKey<T, TProperty>(
    this IRuleBuilderOptions<T, TProperty> rule,
    string key)
  {
    return rule.Configure((Action<PropertyRule>) (config => config.CurrentValidator.ErrorMessageSource = (IStringSource) new LanguageStringSource(key)));
  }

  public static void AssertValid<T>(this IValidator<T> validator, T instance)
  {
    ValidationResult validationResult = validator.Validate<T>(instance, (IValidatorSelector) null, (string) null);
    if (!validationResult.IsValid)
      throw new ValidationException($"{ValidatorOptions.LanguageManager.GetString("Validation failed")}: {string.Concat(validationResult.Errors.Select<ValidationFailure, string>((Func<ValidationFailure, string>) (x => $"{Environment.NewLine} -- {x.ErrorMessage}")).ToArray<string>())}");
  }

  public static async Task AssertValidAsync<T>(
    this IValidator<T> validator,
    T instance,
    Action<ValidationContext<T>> setValidationContext = null)
  {
    ValidationContext<T> context = new ValidationContext<T>(instance);
    if (setValidationContext != null)
      setValidationContext(context);
    ValidationResult validationResult = await validator.ValidateAsync((ValidationContext) context);
    if (!validationResult.IsValid)
      throw new ValidationException($"{ValidatorOptions.LanguageManager.GetString("Validation failed")}: {string.Concat(validationResult.Errors.Select<ValidationFailure, string>((Func<ValidationFailure, string>) (x => $"{Environment.NewLine} -- {x.ErrorMessage}")).ToArray<string>())}");
  }
}
