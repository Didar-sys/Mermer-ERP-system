// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.Validators.PartnerTransferLineValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.CRM.Models.Validators;

public class PartnerTransferLineValidator : AbstractValidator<PartnerTransferLine>
{
  public PartnerTransferLineValidator()
  {
    this.RuleFor<string>((Expression<Func<PartnerTransferLine, string>>) (x => x.Id)).NotEmpty<PartnerTransferLine, string>();
    this.RuleFor<string>((Expression<Func<PartnerTransferLine, string>>) (x => x.OfficeId)).NotEmpty<PartnerTransferLine, string>();
    this.RuleFor<string>((Expression<Func<PartnerTransferLine, string>>) (x => x.PartnerId)).NotEmpty<PartnerTransferLine, string>();
    this.RuleFor<Decimal>((Expression<Func<PartnerTransferLine, Decimal>>) (x => x.CreditAmount)).GreaterThanOrEqualTo<PartnerTransferLine, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<PartnerTransferLine, string>>) (x => x.CreditCurrencyId)).NotEmpty<PartnerTransferLine, string>().When<PartnerTransferLine, string>((Func<PartnerTransferLine, bool>) (x => x.CreditAmount > 0M));
    this.RuleFor<Decimal>((Expression<Func<PartnerTransferLine, Decimal>>) (x => x.DebitAmount)).GreaterThanOrEqualTo<PartnerTransferLine, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<PartnerTransferLine, string>>) (x => x.DebitCurrencyId)).NotEmpty<PartnerTransferLine, string>().When<PartnerTransferLine, string>((Func<PartnerTransferLine, bool>) (x => x.DebitAmount > 0M));
  }
}
