// Decompiled with JetBrains decompiler
// Type: Mermer.Authorization.Models.Validators.RoleValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Authorization.Models.Validators;

public class RoleValidator : AbstractValidator<Role>
{
  public RoleValidator()
  {
    this.RuleFor<string>((Expression<Func<Role, string>>) (x => x.Id)).NotEmpty<Role, string>();
    this.RuleFor<string>((Expression<Func<Role, string>>) (x => x.Name)).NotEmpty<Role, string>();
    this.RuleFor<Dictionary<string, int>>((Expression<Func<Role, Dictionary<string, int>>>) (x => x.Authorizations)).Must<Role, Dictionary<string, int>>((Func<Dictionary<string, int>, bool>) (x => x == null || x.All<KeyValuePair<string, int>>((Func<KeyValuePair<string, int>, bool>) (i => !string.IsNullOrEmpty(i.Key))))).WithLocalizationMessageKey<Role, Dictionary<string, int>>("Action id from '{PropertyName}' can not be empty!");
  }
}
