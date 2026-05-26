// Decompiled with JetBrains decompiler
// Type: Mermer.CRM.Models.Validators.PartnerSlipLineValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.CRM.Models.Validators;

public class PartnerSlipLineValidator : AbstractValidator<PartnerSlipLine>
{
  public PartnerSlipLineValidator()
  {
    this.RuleFor<string>((Expression<Func<PartnerSlipLine, string>>) (x => x.Id)).NotEmpty<PartnerSlipLine, string>().WithLocalizationMessageKey<PartnerSlipLine, string>("{Id} cannot be empty!");
    this.RuleFor<string>((Expression<Func<PartnerSlipLine, string>>) (x => x.PartnerId)).NotEmpty<PartnerSlipLine, string>().WithLocalizationMessageKey<PartnerSlipLine, string>("{PropertyName} cannot be empty!");
    this.RuleFor<Decimal>((Expression<Func<PartnerSlipLine, Decimal>>) (x => x.CreditAmount)).GreaterThanOrEqualTo<PartnerSlipLine, Decimal>(0M).WithLocalizationMessageKey<PartnerSlipLine, Decimal>("{PropertyName} cannot be negative!");
    this.RuleFor<string>((Expression<Func<PartnerSlipLine, string>>) (x => x.CreditCurrencyId)).NotEmpty<PartnerSlipLine, string>().When<PartnerSlipLine, string>((Func<PartnerSlipLine, bool>) (x => x.CreditAmount > 0M)).WithLocalizationMessageKey<PartnerSlipLine, string>("{PropertyName} must be set!");
    this.RuleFor<Decimal>((Expression<Func<PartnerSlipLine, Decimal>>) (x => x.DebitAmount)).GreaterThanOrEqualTo<PartnerSlipLine, Decimal>(0M).WithLocalizationMessageKey<PartnerSlipLine, Decimal>("{PropertyName} cannot be negative!");
    this.RuleFor<string>((Expression<Func<PartnerSlipLine, string>>) (x => x.DebitCurrencyId)).NotEmpty<PartnerSlipLine, string>().When<PartnerSlipLine, string>((Func<PartnerSlipLine, bool>) (x => x.DebitAmount > 0M)).WithLocalizationMessageKey<PartnerSlipLine, string>("{PropertyName} must be set!");
  }
}
