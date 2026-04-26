// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Common.Authorizers.ReadOnlyListAuthorizerWithAccountFilter`1
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using Payhas.Binyat.Authorization.Enums;
using Payhas.Binyat.Authorization.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Common.Authorizers;

public abstract class ReadOnlyListAuthorizerWithAccountFilter<T> : ReadOnlyListAuthorizer<T>
{
  protected readonly ILoginService LoginService;

  protected ReadOnlyListAuthorizerWithAccountFilter(
    ILoginService loginService,
    IAuthorizationService authService,
    Enum action)
    : base(authService, action)
  {
    this.LoginService = loginService;
  }

  public override Expression<Func<T, bool>> AuthorizedListFilter()
  {
    return this.LoginService.Session.IsAdmin ? (Expression<Func<T, bool>>) null : this.GetFilter(this.AuthService.GetAccessableAccounts(AccountAccessLevel.Read));
  }

  protected abstract Expression<Func<T, bool>> GetFilter(IEnumerable<string> accounts);
}
