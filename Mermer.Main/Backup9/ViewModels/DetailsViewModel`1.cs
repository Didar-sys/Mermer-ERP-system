// Decompiled with JetBrains decompiler
// Type: Mermer.Mvvm.ViewModels.DetailsViewModel`1
// Assembly: Mermer.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Mvvm.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Data.Authorizers;
using Mermer.Data.Models;
using Mermer.Data.Storage;
using Mermer.Data.Tools;
using Mermer.Mvvm.Services;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Mvvm.ViewModels;

public class DetailsViewModel<T> : BaseViewModel, IMvxViewModel<string>, IMvxViewModel where T : class, INotifyPropertyChanged, IModel
{
  protected readonly IRepository<T> Repository;
  protected readonly IListAuthorizer<T> Authorizer;
  private string _caption;
  private string _subCaption;
  private string _itemId;
  private T _details;
  private bool _isDirty;
  private bool _hasSaveAccess;
  private bool _hasCreateAccess;

  public DetailsViewModel(
    IRepository<T> repository,
    IListAuthorizer<T> authorizer,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(navigationService, userInteractionService)
  {
    this.Repository = repository;
    this.Authorizer = authorizer;
  }

  public override string Caption
  {
    get
    {
      return (this._caption ?? this[typeof (T).Name, Array.Empty<object>()]) + (this.IsDirty ? " *" : "");
    }
    set => this._caption = value;
  }

  public override string SubCaption
  {
    get
    {
      string subCaption = this._subCaption;
      if (subCaption != null)
        return subCaption;
      return this.Details?.ToString();
    }
    set => this._subCaption = value;
  }

  public virtual string ItemId
  {
    get => this._itemId;
    set => this.SetProperty<string>(ref this._itemId, value, nameof (ItemId));
  }

  public virtual T Details
  {
    get => this._details;
    set
    {
      this.SetProperty<T>(ref this._details, value, nameof (Details));
      this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.SubCaption));
    }
  }

  public virtual bool IsDirty
  {
    get => this._isDirty;
    set
    {
      this.SetProperty<bool>(ref this._isDirty, value, nameof (IsDirty));
      this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.Caption));
      this.RaisePropertyChanged<string>((Expression<Func<string>>) (() => this.SubCaption));
    }
  }

  public virtual bool HasSaveAccess
  {
    get => this._hasSaveAccess;
    set => this.SetProperty<bool>(ref this._hasSaveAccess, value, nameof (HasSaveAccess));
  }

  public virtual bool HasCreateAccess
  {
    get => this._hasCreateAccess;
    set => this.SetProperty<bool>(ref this._hasCreateAccess, value, nameof (HasCreateAccess));
  }

  public void Prepare(string parameter) => this.ItemId = parameter;

  protected override Task PreLoad()
  {
    this.HasSaveAccess = string.IsNullOrEmpty(this.ItemId) ? this.Authorizer.CanCreate() : this.Authorizer.CanUpdate();
    this.HasCreateAccess = this.Authorizer.CanCreate();
    return base.PreLoad();
  }

  public override async Task Initialize()
  {
    DetailsViewModel<T> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    detailsViewModel.IsDirty = false;
    if ((object) detailsViewModel.Details == null)
      return;
    // ISSUE: reference to a compiler-generated method
    DirtynessController.ControlDocument<T>(detailsViewModel.Details, new Action<T>(detailsViewModel.\u003CInitialize\u003Eb__33_0));
  }

  protected override async Task OnLoad()
  {
    DetailsViewModel<T> detailsViewModel = this;
    if (!string.IsNullOrEmpty(detailsViewModel.ItemId))
    {
      T async = await detailsViewModel.Repository.GetAsync(detailsViewModel.ItemId);
      detailsViewModel.Details = async;
      if ((object) detailsViewModel.Details == null)
        throw new Exception(detailsViewModel["Item was not found!", Array.Empty<object>()]);
    }
    else
    {
      detailsViewModel.Details = Activator.CreateInstance<T>();
      detailsViewModel.Details.Id = Guid.NewGuid().ToString();
    }
  }

  protected virtual bool CanSave() => !this.IsBusy && this.IsDirty && this.HasSaveAccess;

  public ICommand SaveCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSaveAsync), new Func<bool>(this.CanSave));
    }
  }

  protected virtual async Task<bool> OnSaveAsync()
  {
    DetailsViewModel<T> detailsViewModel = this;
    detailsViewModel.IsBusy = true;
    bool succeed = false;
    try
    {
      if (string.IsNullOrEmpty(detailsViewModel.ItemId))
        await detailsViewModel.Repository.CreateAsync(detailsViewModel.Details);
      else
        await detailsViewModel.Repository.UpdateAsync(detailsViewModel.Details);
      detailsViewModel.ItemId = detailsViewModel.Details.Id;
      detailsViewModel.IsDirty = false;
      succeed = true;
    }
    catch (Exception ex)
    {
      detailsViewModel.UserInteractionService.ShowExceptionMessage(new Exception(string.Format(detailsViewModel["Error saving {0}", new object[1]
      {
        (object) detailsViewModel[typeof (T).Name, Array.Empty<object>()]
      }]), ex));
    }
    detailsViewModel.IsBusy = false;
    return succeed;
  }

  public ICommand SaveAndNewCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSaveAndNewAsync), (Func<bool>) (() => this.CanSave() && this.HasCreateAccess));
    }
  }

  private Task OnSaveAndNewAsync()
  {
    return (Task) this.OnSaveAsync().ContinueWith<Task>((Func<Task<bool>, Task>) (t =>
    {
      if (!t.Result)
        return Task.CompletedTask;
      this.ItemId = string.Empty;
      this.Details = default (T);
      return this.Initialize();
    }));
  }

  public ICommand SaveAndCloseCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSaveAndCloseAsync), new Func<bool>(this.CanSave));
    }
  }

  private Task OnSaveAndCloseAsync()
  {
    return (Task) this.OnSaveAsync().ContinueWith<Task>((Func<Task<bool>, Task>) (t => t.Result ? (Task) this.OnCloseAsync() : Task.CompletedTask));
  }

  public ICommand ReloadCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnReloadAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private Task OnReloadAsync()
  {
    if (this.IsDirty)
    {
      bool? nullable = this.UserInteractionService.ShowMessage(this["Reloading", Array.Empty<object>()], this["Would you like to save?", Array.Empty<object>()], UserInteractionType.YesNoCancel);
      if (!nullable.HasValue)
        return Task.CompletedTask;
      if (nullable.Value)
        return (Task) this.OnSaveAsync().ContinueWith<Task>((Func<Task<bool>, Task>) (t => t.Result ? this.Initialize() : Task.CompletedTask));
    }
    return this.Initialize();
  }

  public override async Task<bool> OnCloseAsync()
  {
    DetailsViewModel<T> detailsViewModel = this;
    if (detailsViewModel.IsDirty)
    {
      bool? nullable = detailsViewModel.UserInteractionService.ShowMessage(detailsViewModel["Closing", Array.Empty<object>()], detailsViewModel["Would you like to save?", Array.Empty<object>()], UserInteractionType.YesNoCancel);
      if (!nullable.HasValue)
        return false;
      if (nullable.Value)
      {
        if (!await detailsViewModel.OnSaveAsync())
          return false;
      }
    }
    // ISSUE: reference to a compiler-generated method
    return await detailsViewModel.\u003C\u003En__1();
  }
}
