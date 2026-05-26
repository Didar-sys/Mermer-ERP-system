// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockNameComposerDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using Mermer.StockManagement.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockNameComposerDetailsViewModel(
  IRepository<StockNameComposer> repository,
  IListAuthorizer<StockNameComposer> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : DetailsViewModel<StockNameComposer>(repository, authorizer, navigationService, userInteractionService)
{
  protected override async Task PostLoad()
  {
    StockNameComposerDetailsViewModel detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (detailsViewModel.Details.Values != null)
      return;
    detailsViewModel.Details.Values = new ObservableCollection<StockNameComposerValue>();
  }
}
