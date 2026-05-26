// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Warehousing.StockTransferPrintTypeDialogViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Ui.Core.Services;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Warehousing;

public class StockTransferPrintTypeDialogViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  DialogViewModel(messenger, navigationService, userInteractionService),
  IMvxViewModelResult<StockTransferPrintingType?>,
  IMvxViewModel
{
  public override Task<bool> OnCloseAsync()
  {
    return this.NavigationService.Close<StockTransferPrintingType?>((IMvxViewModelResult<StockTransferPrintingType?>) this, new StockTransferPrintingType?());
  }

  public ICommand SelectPrintingTypeCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand<StockTransferPrintingType>(new Func<StockTransferPrintingType, Task>(this.OnSelectPrintingTypeCommandAsync), (Func<StockTransferPrintingType, bool>) (x => !this.IsBusy));
    }
  }

  protected virtual Task OnSelectPrintingTypeCommandAsync(StockTransferPrintingType type)
  {
    return (Task) this.NavigationService.Close<StockTransferPrintingType?>((IMvxViewModelResult<StockTransferPrintingType?>) this, new StockTransferPrintingType?(type));
  }
}
