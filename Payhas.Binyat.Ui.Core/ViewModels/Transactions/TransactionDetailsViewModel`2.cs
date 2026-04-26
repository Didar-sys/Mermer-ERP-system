// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Transactions.TransactionDetailsViewModel`2
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Payhas.Binyat.Authorization.Services;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.FundsManagement.Models.Extenders;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Models.Authorizers;
using Payhas.Binyat.Transactions.Services;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Data;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Transactions;

public class TransactionDetailsViewModel<T, TLine> : TransactionDetailsViewModel<T>
  where T : Transaction<TLine>
  where TLine : TransactionLine
{
  protected AppSettings AppSettings;
  protected readonly IConfigurator Configurator;
  private bool _canChangeDate;
  private string[] _groupNames;
  private string[] _tagNames;
  private TLine _selectedLine;

  public TransactionDetailsViewModel(
    IConfigurator configurator,
    IRepository<T> repository,
    IListAuthorizer<T> authorizer,
    ILoginService loginService,
    Reference<Currency> currencies,
    IMvxNavigationService navigationService,
    ITransactionCodeGenerationService codegentor,
    IUserInteractionService userInteractionService)
    : base(codegentor, repository, authorizer, loginService, navigationService, userInteractionService)
  {
    this.Configurator = configurator;
    this.Currencies = currencies;
  }

  protected override MvxInpcInterceptionResult InterceptRaisePropertyChanged(
    PropertyChangedEventArgs changedArgs)
  {
    if (changedArgs.PropertyName == "HasSaveAccess")
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.CanChangeDate));
    return base.InterceptRaisePropertyChanged(changedArgs);
  }

  public override string Caption
  {
    get
    {
      return this[this.Details?.Type ?? typeof (T).Name, Array.Empty<object>()] + (this.IsDirty ? " *" : "");
    }
    set => base.Caption = value;
  }

  public bool CanChangeDate
  {
    get => this.HasSaveAccess && this._canChangeDate;
    set => this.SetProperty<bool>(ref this._canChangeDate, value, nameof (CanChangeDate));
  }

  public Reference<Currency> Currencies { get; }

  public virtual string[] GroupNames
  {
    get => this._groupNames;
    set => this.SetProperty<string[]>(ref this._groupNames, value, nameof (GroupNames));
  }

  public virtual string[] TagNames
  {
    get => this._tagNames;
    set => this.SetProperty<string[]>(ref this._tagNames, value, nameof (TagNames));
  }

  public virtual TLine SelectedLine
  {
    get => this._selectedLine;
    set
    {
      this.SetProperty<TLine>(ref this._selectedLine, value, nameof (SelectedLine));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.IsLineSelected));
      this.RaisePropertyChanged<bool>((Expression<Func<bool>>) (() => this.CanEditSelectedLine));
    }
  }

  public bool IsLineSelected => (object) this.SelectedLine != null;

  public bool CanEditSelectedLine => this.HasSaveAccess && (object) this.SelectedLine != null;

  protected virtual async Task LoadFacetsAsync()
  {
    TransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    if (!(detailsViewModel.Repository is IRepositoryWithFacets<T> repository))
      return;
    string[] strArray = new string[2]
    {
      "GroupNames",
      "TagNames"
    };
    Dictionary<string, Dictionary<string, int>> facets = await repository.GetFacets(strArray);
    detailsViewModel.GroupNames = facets["GroupNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
    detailsViewModel.TagNames = facets["TagNames"].Select<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToArray<string>();
  }

  protected override Task PreLoad()
  {
    this.CanChangeDate = !(this.Authorizer is ITransactionAuthorizer<T> authorizer) || authorizer.CanChangeDate();
    return Task.WhenAll(base.PreLoad(), this.LoadFacetsAsync(), this.Currencies.Initialize(), Task.Run((Func<Task>) (async () => this.AppSettings = await this.Configurator.GetConfigAsync<AppSettings>())));
  }

  protected override async Task PostLoad()
  {
    TransactionDetailsViewModel<T, TLine> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    detailsViewModel.Details.CurrencyConverterRequested += new Payhas.Binyat.Transactions.Models.CurrencyConverter(detailsViewModel.CurrencyConverter);
    detailsViewModel.Details.AmountFormatterRequested += new Payhas.Binyat.Transactions.Models.AmountFormatter(detailsViewModel.AmountFormatter);
    detailsViewModel.Details.DefaultCurrencyIdRequested += new CurrencyId(detailsViewModel.DefaultCurrencyIdProvider);
    detailsViewModel.Details.RaiseChangeEvents = false;
    detailsViewModel.Details.DisplayCurrencyId = detailsViewModel.DefaultCurrencyIdProvider();
    detailsViewModel.Details.PropertyChanged += new PropertyChangedEventHandler(detailsViewModel.Details_PropertyChanged);
    if (detailsViewModel.Details.Lines == null)
      detailsViewModel.Details.Lines = new WatchedObservableCollection<TLine>();
    if (detailsViewModel.Details.CurrencyConvertions == null)
      detailsViewModel.Details.CurrencyConvertions = new WatchedObservableCollection<CurrencyConvertion>();
    detailsViewModel.Details.RaisePropertyChanged("LinesCount");
    WatchedObservableCollection<TLine> lines = detailsViewModel.Details.Lines;
    if (lines != null)
      lines.FirstOrDefault<TLine>()?.UpdateDisplayCurrencyId(false);
    IEnumerable<string> usedCurrencyIds = detailsViewModel.Details.Lines.Select<TLine, string>((Func<TLine, string>) (x => x.CurrencyId)).Distinct<string>();
    detailsViewModel.Currencies.Filter = (Func<Currency, bool>) (x => !x.IsDisabled || usedCurrencyIds.Contains<string>(x.Id));
  }

  protected override Task<bool> OnSaveAsync()
  {
    this.Details.PropertyChanged -= new PropertyChangedEventHandler(this.Details_PropertyChanged);
    return base.OnSaveAsync();
  }

  protected virtual void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "DisplayCurrencyId" && !this.IsBusy && this.Details.LinesCount > 0)
    {
      int num1 = 0;
      if ((object) this.SelectedLine != null)
      {
        int num2 = this.Details.Lines.IndexOf(this.SelectedLine);
        if (num2 > 10)
          num1 = num2 - 10;
      }
      int num3 = num1 + 20;
      if (num3 > this.Details.LinesCount)
        num3 = this.Details.LinesCount;
      for (int index = num1; index < num3; ++index)
        this.Details.Lines.Move(index, index);
    }
    this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.SubCaption));
  }

  protected CurrencyConvertion CurrencyConverter(string currencyId)
  {
    Reference<Currency> currencies = this.Currencies;
    Currency currency1;
    if (currencies == null)
    {
      currency1 = (Currency) null;
    }
    else
    {
      IEnumerable<Currency> list = currencies.List;
      currency1 = list != null ? list.SingleOrDefault<Currency>((Func<Currency, bool>) (x => x.Id == currencyId)) : (Currency) null;
    }
    Currency currency2 = currency1;
    CurrencyRate rate = currency2 != null ? currency2.GetRate(new DateTime?(this.Details.Date)) : (CurrencyRate) null;
    if (rate == null)
      return (CurrencyConvertion) null;
    return new CurrencyConvertion()
    {
      CurrencyId = currency2.Id,
      Multiplier = rate.Multiplier,
      Divider = rate.Divider
    };
  }

  protected string AmountFormatter(Decimal amount, string currencyId)
  {
    Reference<Currency> currencies = this.Currencies;
    Currency currency1;
    if (currencies == null)
    {
      currency1 = (Currency) null;
    }
    else
    {
      IEnumerable<Currency> list = currencies.List;
      currency1 = list != null ? list.SingleOrDefault<Currency>((Func<Currency, bool>) (x => x.Id == currencyId)) : (Currency) null;
    }
    Currency currency2 = currency1;
    return currency2 == null ? (string) null : string.Format("{0:#,##0.00} {1}", (object) amount, (object) currency2.Name);
  }

  private string DefaultCurrencyIdProvider()
  {
    if (!string.IsNullOrEmpty(this.AppSettings?.DefaultCurrencyId))
      return this.AppSettings.DefaultCurrencyId;
    Reference<Currency> currencies = this.Currencies;
    if (currencies == null)
      return (string) null;
    IEnumerable<Currency> list = currencies.List;
    if (list == null)
      return (string) null;
    return list.SingleOrDefault<Currency>((Func<Currency, bool>) (x => x.IsDefault))?.Id;
  }
}
