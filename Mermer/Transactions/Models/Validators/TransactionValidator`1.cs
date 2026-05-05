// Decompiled with JetBrains decompiler
// Type: Mermer.Transactions.Models.Validators.TransactionValidator`1
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Common.Models.Validators;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Transactions.Models.Validators;

public abstract class TransactionValidator<T> : AbstractModelValidator<T> where T : TransactionModel
{
  protected TransactionValidator()
  {
    this.RuleFor<DateTime>((Expression<Func<T, DateTime>>) (x => x.Date)).NotEmpty<T, DateTime>();
    this.RuleFor<string>((Expression<Func<T, string>>) (x => x.Code)).NotEmpty<T, string>();
    this.RuleFor<string>((Expression<Func<T, string>>) (x => x.Type)).NotEmpty<T, string>();
    this.RuleFor<string>((Expression<Func<T, string>>) (x => x.UserId)).NotEmpty<T, string>();
    this.RuleFor<string>((Expression<Func<T, string>>) (x => x.UserName)).NotEmpty<T, string>();
  }
}
