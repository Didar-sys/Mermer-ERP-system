// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.CRM.Models.Authorizers.PartnerActionsAuthorizer
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Authorizers;
using Payhas.Data.Tools.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.CRM.Models.Authorizers;

public class PartnerActionsAuthorizer(ILoginService loginService, IAuthorizationService authService) : 
  ReadOnlyListAuthorizerWithAccountFilter<PartnerAction>(loginService, authService, (Enum) Actions.PartnerActionsList)
{
  public override Enum GetReadAccessLevel() => (Enum) AccessLevel.Grant;

  protected override Expression<Func<PartnerAction, bool>> GetFilter(IEnumerable<string> accounts)
  {
    return Predicate.Create<PartnerAction>((Expression<Func<PartnerAction, bool>>) (x => accounts.Contains<string>(x.ActionOfficeId)));
  }
}
