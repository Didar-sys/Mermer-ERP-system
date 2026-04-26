// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Commerce.InvoicesWithPaymentInfoListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Commerce.Models;
using Payhas.Binyat.Commerce.Services;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Ui.Core.Helpers;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Mvvm.Messages;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using Payhas.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Commerce;

public class InvoicesWithPaymentInfoListViewModel : 
  ListViewModelBaseWithFilterDate<InvoicePaymentInfo>
{
  private readonly IConfigurator _configurator;
  private readonly IInvoicesRepository _repository;
  private readonly MvxSubscriptionToken _messageToken;
  private string _partnerId;
  private string _officeId;
  private bool _initialized;

  public InvoicesWithPaymentInfoListViewModel(
    IMvxMessenger messenger,
    IConfigurator configurator,
    Reference<Office> offices,
    Reference<Partner> partners,
    IInvoicesRepository repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._repository = repository;
    this._configurator = configurator;
    this._messageToken = messenger.Subscribe<DocumentModified<Invoice>>((Action<DocumentModified<Invoice>>) (async m => await this.Initialize()), MvxReference.Strong);
    this.Offices = offices;
    this.Partners = partners;
  }

  public Reference<Office> Offices { get; }

  public Reference<Partner> Partners { get; }

  public override string SubCaption
  {
    get
    {
      System.Collections.Generic.List<string> stringList = new System.Collections.Generic.List<string>();
      if (!string.IsNullOrEmpty(this.OfficeId))
      {
        IEnumerable<Office> list = this.Offices.List;
        Office office = list != null ? list.SingleOrDefault<Office>((Func<Office, bool>) (x => x.Id == this.OfficeId)) : (Office) null;
        if (office != null)
          stringList.Add(office.Name);
      }
      if (!string.IsNullOrEmpty(this.PartnerId))
      {
        IEnumerable<Partner> list = this.Partners.List;
        Partner partner = list != null ? list.SingleOrDefault<Partner>((Func<Partner, bool>) (x => x.Id == this.PartnerId)) : (Partner) null;
        if (partner != null)
          stringList.Add(partner.Name);
      }
      return !stringList.Any<string>() ? base.SubCaption : string.Join(" | ", (IEnumerable<string>) stringList);
    }
  }

  public virtual string PartnerId
  {
    get => this._partnerId;
    set
    {
      if (this.SetProperty<string>(ref this._partnerId, value, nameof (PartnerId)) && !this.IsBusy)
        this.ReInitialize();
      this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.SubCaption));
    }
  }

  public virtual string OfficeId
  {
    get => this._officeId;
    set
    {
      if (this.SetProperty<string>(ref this._officeId, value, nameof (OfficeId)) && !this.IsBusy)
        this.ReInitialize();
      this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.SubCaption));
    }
  }

  private async void ReInitialize() => await this.Initialize();

  protected override Task PreLoad()
  {
    if (!this._initialized)
    {
      this.OfficeId = this._configurator.GetConfig<AppSettings>().DefaultOfficeId;
      this._initialized = true;
    }
    return Task.WhenAll(base.PreLoad(), this.Partners.Initialize(), this.Offices.Initialize());
  }

  protected override Task<int> CountFilteredListByDateAsync(DateTime from, DateTime till)
  {
    return this._repository.CountPaymentInfoAsync(from, till, this.OfficeId, this.PartnerId);
  }

  protected override Task<int> CountFilteredListAsync(ListFilter filter)
  {
    return this._repository.CountPaymentInfoAsync(DateTime.MinValue, DateTime.MaxValue, this.OfficeId, this.PartnerId);
  }

  protected override Task<IEnumerable<InvoicePaymentInfo>> GetFilteredListByDateAsync(
    DateTime from,
    DateTime till)
  {
    return this._repository.GetPaymentInfoAsync(from, till, this.OfficeId, this.PartnerId);
  }

  protected override Task<IEnumerable<InvoicePaymentInfo>> GetFilteredListAsync(ListFilter filter)
  {
    return this._repository.GetPaymentInfoAsync(DateTime.MinValue, DateTime.MaxValue, this.OfficeId, this.PartnerId);
  }

  protected override Expression<Func<InvoicePaymentInfo, bool>> GetDateFilter(
    DateTime from,
    DateTime till)
  {
    throw new NotImplementedException();
  }

  protected override Task<int> CountListAsync(
    params Expression<Func<InvoicePaymentInfo, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }

  protected override Task<IEnumerable<InvoicePaymentInfo>> GetListAsync(
    params Expression<Func<InvoicePaymentInfo, bool>>[] predicates)
  {
    throw new NotImplementedException();
  }

  public ICommand SelectOrViewDetailsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectOrViewDetailsAsync), (Func<bool>) (() => !this.IsBusy && this.SelectedItem != null));
    }
  }

  private Task OnSelectOrViewDetailsAsync()
  {
    return this.NavigationService.Navigate<DetailsViewModel<Invoice>, string>(this.SelectedItem.Id);
  }

  public override void Dispose()
  {
    base.Dispose();
    this._messageToken.Dispose();
  }
}
