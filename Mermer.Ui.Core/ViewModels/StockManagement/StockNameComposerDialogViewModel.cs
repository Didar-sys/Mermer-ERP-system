// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.StockManagement.StockNameComposerDialogViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.StockManagement.Models;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockNameComposerDialogViewModel : 
  DialogViewModel,
  IMvxViewModel<SncParams, SncParams>,
  IMvxViewModel<SncParams>,
  IMvxViewModel,
  IMvxViewModelResult<SncParams>
{
  private readonly IRepository<StockNameComposer> _repository;
  private IEnumerable<StockNameComposer> _composers;
  private IEnumerable<StockNameComposerValue> _values;

  public StockNameComposerDialogViewModel(
    IMvxMessenger messenger,
    IRepository<StockNameComposer> repository,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._repository = repository;
  }

  public IEnumerable<StockNameComposer> Composers
  {
    get => this._composers;
    set
    {
      this.SetProperty<IEnumerable<StockNameComposer>>(ref this._composers, value, nameof (Composers));
    }
  }

  public IEnumerable<StockNameComposerValue> Values
  {
    get => this._values;
    set
    {
      this.SetProperty<IEnumerable<StockNameComposerValue>>(ref this._values, value, nameof (Values));
    }
  }

  public SncParams CurrentValue { get; private set; }

  public void Prepare(SncParams parameter) => this.CurrentValue = parameter;

  public override async Task Initialize()
  {
    await base.Initialize();
    this.Composers = (IEnumerable<StockNameComposer>) (await this._repository.GetAsync()).Where<StockNameComposer>((Func<StockNameComposer, bool>) (x => !x.IsDisabled)).OrderBy<StockNameComposer, int>((Func<StockNameComposer, int>) (x => x.Order));
  }

  public ICommand Compose
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnComposeAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

    private Task OnComposeAsync()
    {
        // 1. Захист: якщо нічого не вибрано, не даємо закрити вікно і згенерувати пусту назву
        if (this.Values == null || !this.Values.Any())
        {
            this.UserInteractionService.ShowMessage(this["Error", Array.Empty<object>()], this["Please select at least one value", Array.Empty<object>()]);
            return Task.CompletedTask;
        }

        // 2. Склеиваем название из безопасных (проверенных) значений. 
        // (Я немного почистил код от декомпилированного мусора для лучшей читабельности)
        return (Task)this.NavigationService.Close<SncParams>((IMvxViewModelResult<SncParams>)this, new SncParams()
        {
            ShortName = string.Join(" ", this.Values.Select(x => x.ShortName)),
            Name = string.Join(" ", this.Values.Select(x => x.Name))
        });
    }

    public override Task<bool> OnCloseAsync()
  {
    return this.NavigationService.Close<SncParams>((IMvxViewModelResult<SncParams>) this, (SncParams) null);
  }
}
