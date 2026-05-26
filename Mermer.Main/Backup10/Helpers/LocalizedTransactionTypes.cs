// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.Helpers.LocalizedTransactionTypes
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Localization;
using Mermer.Commerce.Models;
using Mermer.CRM.Models;
using Mermer.Finance.Models;
using Mermer.Warehousing.Models;
using Mermer.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace Mermer.Ui.Core.Helpers;

public class LocalizedTransactionTypes : BindableObject
{
  private IEnumerable<ListHelper<string>> _list;

  public LocalizedTransactionTypes(IMvxLanguageBinder textSource, params string[] additionalTypes)
  {
    this.Initialize(textSource, additionalTypes);
  }

  public IEnumerable<ListHelper<string>> List
  {
    get => this._list;
    set => this.SetProperty<IEnumerable<ListHelper<string>>>(ref this._list, value, nameof (List));
  }

  public void Initialize(IMvxLanguageBinder textSource, params string[] additionalTypes)
  {
    System.Collections.Generic.List<string> source = (additionalTypes != null ? ((IEnumerable<string>) additionalTypes).ToList<string>() : (System.Collections.Generic.List<string>) null) ?? new System.Collections.Generic.List<string>();
    source.AddRange(Enum.GetValues(typeof (FundsSlipType)).Cast<FundsSlipType>().Select<FundsSlipType, string>((Func<FundsSlipType, string>) (x => x.ToString())));
    source.AddRange((IEnumerable<string>) new string[5]
    {
      "FundsTransfer",
      "FundsTransferSource",
      "FundsTransferDestination",
      "DailyFundsRegistery",
      "ExpenseSlip"
    });
    source.AddRange(Enum.GetValues(typeof (StockSlipType)).Cast<StockSlipType>().Select<StockSlipType, string>((Func<StockSlipType, string>) (x => x.ToString())));
    source.AddRange((IEnumerable<string>) new string[6]
    {
      "StockTransfer",
      "StockTransferSource",
      "StockTransferDestination",
      "StockRevision",
      "StockOrder",
      "AggregatedStockOrder"
    });
    source.AddRange(Enum.GetValues(typeof (PartnerSlipType)).Cast<PartnerSlipType>().Select<PartnerSlipType, string>((Func<PartnerSlipType, string>) (x => x.ToString())));
    source.AddRange((IEnumerable<string>) new string[1]
    {
      "PartnerTransfer"
    });
    source.AddRange(Enum.GetValues(typeof (InvoiceType)).Cast<InvoiceType>().Select<InvoiceType, string>((Func<InvoiceType, string>) (x => x.ToString())));
    source.AddRange(Enum.GetValues(typeof (BillType)).Cast<BillType>().Select<BillType, string>((Func<BillType, string>) (x => x.ToString())));
    this.List = source.Distinct<string>().Select<string, ListHelper<string>>((Func<string, ListHelper<string>>) (x => new ListHelper<string>()
    {
      Text = textSource.GetText(x),
      Value = x
    }));
  }
}
