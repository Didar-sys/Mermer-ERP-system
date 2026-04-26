// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning.StockRevisionDetailsReportViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Models.Extenders;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Warehousing.Revisioning.Models;
using Payhas.Binyat.Warehousing.Revisioning.Services;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Warehousing.Revisioning;

public class StockRevisionDetailsReportViewModel : 
  ListViewModelBase<StockRevisionCountInfoWithData>,
  IMvxViewModel<Tuple<string, DateTime?>>,
  IMvxViewModel
{
  private string _revisionId;
  private DateTime? _revisionFinishDate;
  private readonly IStockRevisionsRepository _repository;
  private string _displayCurrencyId;
  private bool _initialized;

  public StockRevisionDetailsReportViewModel(
    IMvxMessenger messenger,
    Reference<Currency> currencies,
    IStockRevisionsRepository repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._repository = repository;
    this.Currencies = currencies;
    this.PropertyChanged += new PropertyChangedEventHandler(this.VmPropertyChanged);
  }

  public Reference<Currency> Currencies { get; }

  public string DisplayCurrencyId
  {
    get => this._displayCurrencyId;
    set
    {
      if (!this.SetProperty<string>(ref this._displayCurrencyId, value, nameof (DisplayCurrencyId)) || this.IsBusy || this.List == null)
        return;
      foreach (RequestCurrencyConverter currencyConverter in this.List)
        currencyConverter.UpdateDisplayCurrencyId(false);
    }
  }

  public int ExceedsCount
  {
    get
    {
      IEnumerable<StockRevisionCountInfoWithData> list = this.List;
      return list == null ? 0 : list.Count<StockRevisionCountInfoWithData>((Func<StockRevisionCountInfoWithData, bool>) (x => x.TotalDifference > 0M));
    }
  }

  public int EqualsCount
  {
    get
    {
      IEnumerable<StockRevisionCountInfoWithData> list = this.List;
      return list == null ? 0 : list.Count<StockRevisionCountInfoWithData>((Func<StockRevisionCountInfoWithData, bool>) (x => x.TotalDifference == 0M));
    }
  }

  public int DeficitCount
  {
    get
    {
      IEnumerable<StockRevisionCountInfoWithData> list = this.List;
      return list == null ? 0 : list.Count<StockRevisionCountInfoWithData>((Func<StockRevisionCountInfoWithData, bool>) (x => x.TotalDifference < 0M));
    }
  }

  public int AllRecordsCount
  {
    get
    {
      IEnumerable<StockRevisionCountInfoWithData> list = this.List;
      return list == null ? 0 : list.Count<StockRevisionCountInfoWithData>();
    }
  }

  private void VmPropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!(e.PropertyName == "List"))
      return;
    this.RaisePropertyChanged<int>((Expression<Func<int>>) (() => this.ExceedsCount));
    this.RaisePropertyChanged<int>((Expression<Func<int>>) (() => this.EqualsCount));
    this.RaisePropertyChanged<int>((Expression<Func<int>>) (() => this.DeficitCount));
    this.RaisePropertyChanged<int>((Expression<Func<int>>) (() => this.AllRecordsCount));
  }

  public void Prepare(Tuple<string, DateTime?> parameter)
  {
    this._revisionId = parameter.Item1;
    this._revisionFinishDate = parameter.Item2;
  }

  protected override async Task PreLoad()
  {
    await Task.WhenAll(base.PreLoad(), this.Currencies.Initialize());
    if (this._initialized)
      return;
    this.DisplayCurrencyId = this.Currencies.List.Single<Currency>((Func<Currency, bool>) (x => x.IsDefault)).Id;
    this._initialized = true;
  }

  protected override async Task OnLoad()
  {
    StockRevisionDetailsReportViewModel detailsReportViewModel = this;
    StockRevisionCountInfoWithData[] array = (await detailsReportViewModel._repository.GetCountInfosAsync(detailsReportViewModel._revisionId, detailsReportViewModel.DisplayCurrencyId)).ToArray<StockRevisionCountInfoWithData>();
    foreach (StockRevisionCountInfoWithData countInfoWithData in array)
    {
      countInfoWithData.CurrencyConverterRequested += new CurrencyConverter(detailsReportViewModel.GetCurrencyConverter);
      countInfoWithData.DisplayCurrencyIdRequested += new CurrencyId(detailsReportViewModel.GetDisplayCurrencyId);
      countInfoWithData.UpdateCurrencyConvertion();
      countInfoWithData.UpdateDisplayCurrencyId(false);
    }
    detailsReportViewModel.List = (IEnumerable<StockRevisionCountInfoWithData>) array;
  }

  private string GetDisplayCurrencyId() => this.DisplayCurrencyId;

  private CurrencyConvertion GetCurrencyConverter(string currencyId)
  {
    Currency currency = this.Currencies.List.Single<Currency>((Func<Currency, bool>) (x => x.Id == currencyId));
    CurrencyRate rate = currency.GetRate(this._revisionFinishDate);
    return new CurrencyConvertion()
    {
      CurrencyId = currency.Id,
      Multiplier = rate.Multiplier,
      Divider = rate.Divider
    };
  }
}
