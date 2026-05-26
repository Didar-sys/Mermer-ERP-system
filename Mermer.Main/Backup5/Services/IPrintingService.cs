// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Services.IPrintingService
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Mermer.Commerce.Models;
using Mermer.Finance.Spending.Models;
using Mermer.Reporting.Models;
using Mermer.Warehousing.Models;
using Mermer.Warehousing.Ordering.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.Services;

public interface IPrintingService
{
  IEnumerable<string> GetPrinterNames();

  Task PrintStockSlip(StockSlip item, bool force = false);

  Task PrintStockTransfer(StockTransfer item, bool force = false);

  Task PrintStockOrder(StockOrder item, bool force = false);

  Task PrintExpenseSlip(ExpenseSlip item, bool force = false);

  Task PrintBill(Bill item, Decimal partnerPrevBalance, bool force = false);

  Task PrintInvoice(Invoice item, Decimal partnerPrevBalance, bool force = false);

  Task PrintAggregatedReport(
    AggregatedReport data,
    DateTime from,
    DateTime till,
    string[] offices);

  Task PrintBarcodes(string title, string barcode, string price, int copiesCount);
}
