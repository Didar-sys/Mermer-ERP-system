// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Models.Validators.UserValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Authorization.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Authorization.Models.Validators;

public class UserValidator : AbstractValidator<User>
{
  public UserValidator()
  {
    this.RuleFor<string>((Expression<Func<User, string>>) (x => x.Id)).NotEmpty<User, string>();
    this.RuleFor<string>((Expression<Func<User, string>>) (x => x.Username)).NotEmpty<User, string>();
    this.RuleFor<string>((Expression<Func<User, string>>) (x => x.Password)).NotEmpty<User, string>();
    this.RuleFor<IEnumerable<string>>((Expression<Func<User, IEnumerable<string>>>) (x => x.Roles)).Must<User, IEnumerable<string>>((Func<IEnumerable<string>, bool>) (x => x == null || x.All<string>((Func<string, bool>) (i => !string.IsNullOrEmpty(i))))).WithLocalizationMessageKey<User, IEnumerable<string>>("Role id from '{PropertyName}' can not be empty!");
    this.RuleFor<Dictionary<string, AccountAccessLevel>>((Expression<Func<User, Dictionary<string, AccountAccessLevel>>>) (x => x.AccountPrivileges)).Must<User, Dictionary<string, AccountAccessLevel>>((Func<Dictionary<string, AccountAccessLevel>, bool>) (x => x == null || x.All<KeyValuePair<string, AccountAccessLevel>>((Func<KeyValuePair<string, AccountAccessLevel>, bool>) (i => !string.IsNullOrEmpty(i.Key))))).WithLocalizationMessageKey<User, Dictionary<string, AccountAccessLevel>>("Account id from '{PropertyName}' can not be empty!");
  }
}
