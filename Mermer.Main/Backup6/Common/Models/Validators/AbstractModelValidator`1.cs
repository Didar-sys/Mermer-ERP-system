// Decompiled with JetBrains decompiler
// Type: Mermer.Common.Models.Validators.AbstractModelValidator`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Data.Models;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Common.Models.Validators;

public abstract class AbstractModelValidator<T> : AbstractValidator<T> where T : IModel
{
  protected AbstractModelValidator()
  {
    this.RuleFor<string>((Expression<Func<T, string>>) (x => x.Id)).NotEmpty<T, string>();
  }
}
