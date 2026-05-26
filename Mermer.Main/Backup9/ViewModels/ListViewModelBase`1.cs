// Decompiled with JetBrains decompiler
// Type: Mermer.Mvvm.ViewModels.ListViewModelBase`1
// Assembly: Mermer.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Mvvm.dll

using Humanizer;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Mvvm.Messages;
using Mermer.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Mvvm.ViewModels;

public abstract class ListViewModelBase<T> : BaseViewModel
{
  private readonly MvxSubscriptionToken _messageToken;
  private string _caption;
  private string _subCaption;
  private IEnumerable<T> _list;
  private T _selectedItem;

  protected ListViewModelBase(
    IMvxMessenger messenger,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(navigationService, userInteractionService)
  {
    this._messageToken = messenger.Subscribe<DocumentModified<T>>((Action<DocumentModified<T>>) (async m => await this.Initialize()), MvxReference.Strong);
  }

  public override string Caption
  {
    get => this._caption ?? this[typeof (T).Name.Pluralize(), Array.Empty<object>()];
    set => this._caption = value;
  }

  public override string SubCaption
  {
    get => this._subCaption ?? this["All Records", Array.Empty<object>()];
    set => this.SetProperty<string>(ref this._subCaption, value, nameof (SubCaption));
  }

  public virtual IEnumerable<T> List
  {
    get => this._list;
    set => this.SetProperty<IEnumerable<T>>(ref this._list, value, nameof (List));
  }

  public T SelectedItem
  {
    get => this._selectedItem;
    set => this.SetProperty<T>(ref this._selectedItem, value, nameof (SelectedItem));
  }

  public ICommand ReloadCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnReloadAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private Task OnReloadAsync() => this.Initialize();

  public override void Dispose()
  {
    base.Dispose();
    this._messageToken?.Dispose();
  }
}
