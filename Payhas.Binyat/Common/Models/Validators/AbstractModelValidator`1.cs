// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Common.Models.Validators.AbstractModelValidator`1
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Data.Models;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Common.Models.Validators;

public abstract class AbstractModelValidator<T> : AbstractValidator<T> where T : IModel
{
  protected AbstractModelValidator()
  {
    this.RuleFor<string>((Expression<Func<T, string>>) (x => x.Id)).NotEmpty<T, string>();
  }
}
