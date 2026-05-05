// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Transactions.TransactionLineEditViewModel`2
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
namespace Mermer.Ui.Core.ViewModels.Transactions;

public abstract class TransactionLineEditViewModel<TParams, TResult>(
  IMvxMessenger messenger,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : 
  DialogViewModel(messenger, navigationService, userInteractionService),
  IMvxViewModel<TParams, TResult>,
  IMvxViewModel<TParams>,
  IMvxViewModel,
  IMvxViewModelResult<TResult>
  where TParams : TResult
{
  private TParams _details;

  public TParams Details
  {
    get => this._details;
    set => this.SetProperty<TParams>(ref this._details, value, nameof (Details));
  }

  public virtual void Prepare(TParams parameter) => this.Details = parameter;

  public ICommand SaveCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSaveAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual Task OnSaveAsync()
  {
    return (Task) this.NavigationService.Close<TResult>((IMvxViewModelResult<TResult>) this, (TResult) this.Details);
  }

  public override Task<bool> OnCloseAsync()
  {
    return this.NavigationService.Close<TResult>((IMvxViewModelResult<TResult>) this, default (TResult));
  }
}
