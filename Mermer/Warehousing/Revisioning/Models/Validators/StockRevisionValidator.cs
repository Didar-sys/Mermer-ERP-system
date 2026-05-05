// Decompiled with JetBrains decompiler
// Type: Mermer.Warehousing.Revisioning.Models.Validators.StockRevisionValidator
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using FluentValidation;
using Mermer.Transactions.Models.Validators;
using System;
using System.Linq.Expressions;

#nullable disable
namespace Mermer.Warehousing.Revisioning.Models.Validators;

public class StockRevisionValidator : TransactionValidator<StockRevision>
{
  public StockRevisionValidator()
  {
    this.RuleFor<string>((Expression<Func<StockRevision, string>>) (x => x.WarehouseId)).NotEmpty<StockRevision, string>();
  }
}
