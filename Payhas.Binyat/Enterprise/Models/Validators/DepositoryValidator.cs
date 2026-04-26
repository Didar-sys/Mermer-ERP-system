// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Enterprise.Models.Validators.DepositoryValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Enterprise.Models.Validators;

public class DepositoryValidator : AbstractValidator<Depository>
{
  public DepositoryValidator()
  {
    this.RuleFor<string>((Expression<Func<Depository, string>>) (x => x.Id)).NotEmpty<Depository, string>();
    this.RuleFor<string>((Expression<Func<Depository, string>>) (x => x.OfficeId)).NotEmpty<Depository, string>();
    this.RuleFor<string>((Expression<Func<Depository, string>>) (x => x.Name)).NotEmpty<Depository, string>();
  }
}
