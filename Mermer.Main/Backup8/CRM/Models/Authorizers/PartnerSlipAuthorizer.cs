// Decompiled with JetBrains decompiler
// Type: Mermer.CRM.Models.Authorizers.PartnerSlipAuthorizer
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Transactions.Models.Authorizers;
using Mermer.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.CRM.Models.Authorizers;

public class PartnerSlipAuthorizer(
  ILoginService loginService,
  ILocalizationService localizationService,
  IAuthorizationService authService) : TransactionAuthorizer<PartnerSlip>(loginService, localizationService, authService, (Enum) TransactionActions.PartnerSlips)
{
  protected override string[] GetAccessedAccounts(PartnerSlip item)
  {
    return new string[1]{ item.OfficeId };
  }

  protected override Expression<Func<PartnerSlip, bool>> GetFilter(IEnumerable<string> accounts)
  {
    return Predicate.Create<PartnerSlip>((Expression<Func<PartnerSlip, bool>>) (x => accounts.Contains<string>(x.OfficeId)));
  }
}
