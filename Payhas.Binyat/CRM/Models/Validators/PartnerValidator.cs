// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.Validators.PartnerValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.Common.Models.Validators;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.CRM.Models.Validators;

public class PartnerValidator : AbstractModelValidator<Partner>
{
  public PartnerValidator()
  {
    this.RuleFor<string>((Expression<Func<Partner, string>>) (x => x.Code)).NotEmpty<Partner, string>();
    this.RuleFor<string>((Expression<Func<Partner, string>>) (x => x.Name)).NotEmpty<Partner, string>();
    this.RuleFor<Decimal?>((Expression<Func<Partner, Decimal?>>) (x => x.CreditLimit)).Must<Partner, Decimal?>((Func<Decimal?, bool>) (x => x.GetValueOrDefault() >= 0M));
  }
}
