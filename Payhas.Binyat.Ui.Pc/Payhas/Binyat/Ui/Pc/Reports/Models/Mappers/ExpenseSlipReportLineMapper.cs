// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Pc.Reports.Models.Mappers.ExpenseSlipReportLineMapper
// Assembly: Payhas.Binyat.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Pc.exe

using Payhas.Binyat.Finance.Spending.Models;
using Payhas.Binyat.Ui.Pc.Reports.Helpers;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Pc.Reports.Models.Mappers;

public class ExpenseSlipReportLineMapper(NameHelper nameHelper) : 
  FundsTransactionReportLineMapper<ExpenseSlipLine, ExpenseSlipReportLine>(nameHelper)
{
  public override async Task<ExpenseSlipReportLine> Map(ExpenseSlipLine source)
  {
    ExpenseSlipReportLineMapper reportLineMapper = this;
    // ISSUE: reference to a compiler-generated method
    ExpenseSlipReportLine destination = await reportLineMapper.\u003C\u003En__0(source);
    ExpenseSlipReportLine expenseSlipReportLine = destination;
    expenseSlipReportLine.Expense = await reportLineMapper.NameHelper.GetStockName(source.ExpenseId);
    expenseSlipReportLine = (ExpenseSlipReportLine) null;
    ExpenseSlipReportLine expenseSlipReportLine1 = destination;
    destination = (ExpenseSlipReportLine) null;
    return expenseSlipReportLine1;
  }
}
