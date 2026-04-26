// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Warehousing.Revisioning.Models.Validators.StockRevisionLineValidator
// Assembly: Payhas.Binyat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.dll

using FluentValidation;
using Payhas.Binyat.Common.Models.Validators;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Payhas.Binyat.Warehousing.Revisioning.Models.Validators;

public class StockRevisionLineValidator : AbstractModelValidator<StockRevisionLine>
{
  public StockRevisionLineValidator()
  {
    this.RuleFor<string>((Expression<Func<StockRevisionLine, string>>) (x => x.StockRevisionId)).NotEmpty<StockRevisionLine, string>();
    this.RuleFor<string>((Expression<Func<StockRevisionLine, string>>) (x => x.StockId)).NotEmpty<StockRevisionLine, string>();
    this.RuleFor<DateTime>((Expression<Func<StockRevisionLine, DateTime>>) (x => x.Date)).NotEmpty<StockRevisionLine, DateTime>();
    this.RuleFor<Decimal>((Expression<Func<StockRevisionLine, Decimal>>) (x => x.Quantity)).GreaterThanOrEqualTo<StockRevisionLine, Decimal>(0M);
    this.RuleFor<string>((Expression<Func<StockRevisionLine, string>>) (x => x.UnitId)).NotEmpty<StockRevisionLine, string>();
    this.RuleFor<string>((Expression<Func<StockRevisionLine, string>>) (x => x.UserId)).NotEmpty<StockRevisionLine, string>();
    this.RuleFor<string>((Expression<Func<StockRevisionLine, string>>) (x => x.UserName)).NotEmpty<StockRevisionLine, string>();
  }
}
