// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.FundsManagement.CurrencyDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using Mermer.FundsManagement.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.FundsManagement;

public class CurrencyDetailsViewModel(
  IRepository<Currency> repository,
  IListAuthorizer<Currency> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : DetailsViewModel<Currency>(repository, authorizer, navigationService, userInteractionService)
{
  protected override async Task PostLoad()
  {
    CurrencyDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (detailsViewModel.Details.Rates == null)
      detailsViewModel.Details.Rates = new ObservableCollection<CurrencyRate>();
    detailsViewModel.Details.PropertyChanged += new PropertyChangedEventHandler(detailsViewModel.Details_PropertyChanged);
  }

  private void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!(e.PropertyName == "IsDefault"))
      return;
    if (this.Details.IsDefault)
    {
      Currency details = this.Details;
      ObservableCollection<CurrencyRate> observableCollection = new ObservableCollection<CurrencyRate>();
      observableCollection.Add(new CurrencyRate()
      {
        ValidFrom = DateTime.Today,
        Multiplier = 1M,
        Divider = 1M
      });
      details.Rates = observableCollection;
    }
    else
      this.Details.Rates = new ObservableCollection<CurrencyRate>();
  }
}
