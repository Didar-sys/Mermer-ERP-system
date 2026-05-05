// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Common.PrintKindDialogViewModel
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
namespace Mermer.Ui.Core.ViewModels.Common;

public class PrintKindDialogViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  DialogViewModel(messenger, navigationService, userInteractionService),
  IMvxViewModel<PrintingPreferencesRequest, PrintingPreferencesResult>,
  IMvxViewModel<PrintingPreferencesRequest>,
  IMvxViewModel,
  IMvxViewModelResult<PrintingPreferencesResult>
{
  private PrintKind _availablePrintKinds;
  private bool _printOnSave;
  private bool _setAsDefaultPrintKind;

  public bool CanPrintCheque => this._availablePrintKinds.HasFlag((Enum) PrintKind.Cheque);

  public bool CanPrintSplit => this._availablePrintKinds.HasFlag((Enum) PrintKind.Split);

  public bool CanPrintStandard => this._availablePrintKinds.HasFlag((Enum) PrintKind.Standard);

  public virtual bool PrintOnSave
  {
    get => this._printOnSave;
    set => this.SetProperty<bool>(ref this._printOnSave, value, nameof (PrintOnSave));
  }

  public virtual bool SetAsDefaultPrintKind
  {
    get => this._setAsDefaultPrintKind;
    set
    {
      this.SetProperty<bool>(ref this._setAsDefaultPrintKind, value, nameof (SetAsDefaultPrintKind));
    }
  }

  public void Prepare(PrintingPreferencesRequest parameter)
  {
    this._availablePrintKinds = parameter.AvailablePrintKinds;
    this.PrintOnSave = parameter.PrintOnSave;
  }

  public override Task<bool> OnCloseAsync()
  {
    return this.NavigationService.Close<PrintingPreferencesResult>((IMvxViewModelResult<PrintingPreferencesResult>) this, (PrintingPreferencesResult) null);
  }

  public ICommand SelectPrintKindCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand<PrintKind>(new Func<PrintKind, Task>(this.OnSelectPrintKindCommandAsync), (Func<PrintKind, bool>) (x => !this.IsBusy && this._availablePrintKinds.HasFlag((Enum) x)));
    }
  }

  protected virtual Task OnSelectPrintKindCommandAsync(PrintKind kind)
  {
    IMvxNavigationService navigationService = this.NavigationService;
    PrintingPreferencesResult result = new PrintingPreferencesResult();
    result.PrintKind = new PrintKind?(kind);
    result.PrintOnSave = this.PrintOnSave;
    result.SetAsDefaultPrintKind = this.SetAsDefaultPrintKind;
    return (Task) navigationService.Close<PrintingPreferencesResult>((IMvxViewModelResult<PrintingPreferencesResult>) this, result);
  }
}
