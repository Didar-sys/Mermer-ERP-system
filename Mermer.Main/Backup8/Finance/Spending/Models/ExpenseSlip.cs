// Decompiled with JetBrains decompiler
// Type: Mermer.Finance.Spending.Models.ExpenseSlip
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Transactions.Models;

#nullable disable
namespace Mermer.Finance.Spending.Models;

public class ExpenseSlip : FundsTransaction<ExpenseSlipLine>
{
  public override bool IsFundsIncome => false;

  public override string Type => nameof (ExpenseSlip);
}
