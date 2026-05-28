// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Pc.Reports.Models.Mappers.ExpenseSlipReportLineMapper
// Assembly: Mermer.Ui.Pc, Version=1.4.4.0, Culture=neutral, PublicKeyToken=null
// MVID: D54C0BF8-E817-4120-9485-68C30ADFDFE4
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Pc.exe

using Mermer.Finance.Spending.Models;
using Mermer.Ui.Pc.Reports.Helpers;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Pc.Reports.Models.Mappers;

public class ExpenseSlipReportLineMapper(NameHelper nameHelper) :
    FundsTransactionReportLineMapper<ExpenseSlipLine, ExpenseSlipReportLine>(nameHelper)
{
    public override async Task<ExpenseSlipReportLine> Map(ExpenseSlipLine source)
    {
        ExpenseSlipReportLine destination = await base.Map(source);
        destination.Expense = await NameHelper.GetStockName(source.ExpenseId);
        return destination;
    }
}