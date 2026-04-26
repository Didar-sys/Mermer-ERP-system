// Decompiled with JetBrains decompiler
// Type: Payhas.Mvvm.ViewModels.ListViewModel`1
// Assembly: Payhas.Mvvm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3EAA5570-F618-4E39-B929-F7374F99B43D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Mvvm.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Data.Authorizers;
using Payhas.Data.Models;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Mvvm.ViewModels;

public class ListViewModel<T> : 
  ListViewModelBase<T>,
  IMvxViewModel<string, string>,
  IMvxViewModel<string>,
  IMvxViewModel,
  IMvxViewModelResult<string>
  where T : class, INotifyPropertyChanged, IModel
{
  protected string ItemId;
  protected readonly IRepository<T> Repository;
  protected readonly IListAuthorizer<T> Authorizer;

  public ListViewModel(
    IRepository<T> repository,
    IListAuthorizer<T> authorizer,
    IMvxMessenger messenger,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this.Repository = repository;
    this.Authorizer = authorizer;
  }

  public void Prepare(string parameter) => this.ItemId = parameter;

  protected override async Task OnLoad()
  {
    ListViewModel<T> listViewModel = this;
    IEnumerable<T> async = await listViewModel.Repository.GetAsync();
    listViewModel.List = async;
    if (string.IsNullOrEmpty(listViewModel.ItemId))
      return;
    // ISSUE: reference to a compiler-generated method
    listViewModel.SelectedItem = listViewModel.List.SingleOrDefault<T>(new Func<T, bool>(listViewModel.\u003COnLoad\u003Eb__5_0));
  }

  public bool HasCreateAccess => this.Authorizer.CanCreate();

  public ICommand CreateNewCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCreateNewAsync), (Func<bool>) (() => !this.IsBusy && this.HasCreateAccess));
    }
  }

  protected virtual Task OnCreateNewAsync()
  {
    return this.NavigationService.Navigate<DetailsViewModel<T>, string>(string.Empty);
  }

  public ICommand ViewDetailsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnViewDetailsAsync), (Func<bool>) (() => !this.IsBusy && (object) this.SelectedItem != null));
    }
  }

  protected virtual Task OnViewDetailsAsync()
  {
    return this.NavigationService.Navigate<DetailsViewModel<T>, string>(this.SelectedItem.Id);
  }

  public ICommand SelectOrViewDetailsCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectOrViewDetailsAsync), (Func<bool>) (() => !this.IsBusy && (object) this.SelectedItem != null));
    }
  }

  protected virtual Task OnSelectOrViewDetailsAsync()
  {
    if (!string.IsNullOrEmpty(this.ItemId))
      return (Task) this.NavigationService.Close<string>((IMvxViewModelResult<string>) this, this.SelectedItem.Id);
    this.ViewDetailsCommand.Execute((object) null);
    return Task.CompletedTask;
  }
}
