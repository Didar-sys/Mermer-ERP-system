// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Commerce.InvoicePaymentDialogViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Commerce;

public class InvoicePaymentDialogViewModel(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  DialogViewModel(messenger, navigationService, userInteractionService),
  IMvxViewModel<IpdParams, IpdParams>,
  IMvxViewModel<IpdParams>,
  IMvxViewModel,
  IMvxViewModelResult<IpdParams>
{
  private IpdParams _details;

  public virtual IpdParams Details
  {
    get => this._details;
    set => this.SetProperty<IpdParams>(ref this._details, value, nameof (Details));
  }

  public void Prepare(IpdParams parameter) => this.Details = parameter;

  public ICommand RoundingDiscountCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnRoundingDiscountCommand), (Func<bool>) (() => !this.IsBusy));
    }
  }

  public void OnRoundingDiscountCommand()
  {
    Decimal num = Math.Round(this.Details.SubTotal, 0);
    if (num > this.Details.SubTotal)
      --num;
    this.Details.DiscountsTotal = this.Details.SubTotal - num;
  }

  public ICommand FillPaymentCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnFillPaymentCommand), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private void OnFillPaymentCommand() => this.Details.PaymentsTotal += this.Details.LeftTotal;

  public ICommand FillChangesCommand
  {
    get
    {
      return (ICommand) new MvxCommand(new Action(this.OnFillChangesCommand), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private void OnFillChangesCommand()
  {
    if (this.Details.ChangesTotal > this.Details.LeftTotal)
      this.Details.ChangesTotal -= this.Details.LeftTotal;
    else
      this.Details.ChangesTotal = 0M;
  }

  public ICommand UpdateCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnUpdateCommandAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private Task OnUpdateCommandAsync()
  {
    return (Task) this.NavigationService.Close<IpdParams>((IMvxViewModelResult<IpdParams>) this, this.Details);
  }

  public override Task<bool> OnCloseAsync()
  {
    return this.NavigationService.Close<IpdParams>((IMvxViewModelResult<IpdParams>) this, (IpdParams) null);
  }
}
