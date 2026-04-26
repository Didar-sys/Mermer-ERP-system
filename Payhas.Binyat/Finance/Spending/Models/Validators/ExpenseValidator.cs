// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Finance.Spending.Models.Validators.ExpenseValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.Common.Models.Validators;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Finance.Spending.Models.Validators;

public class ExpenseValidator : AbstractModelValidator<Expense>
{
  public ExpenseValidator()
  {
    this.RuleFor<string>((Expression<Func<Expense, string>>) (x => x.Name)).NotEmpty<Expense, string>();
  }
}
