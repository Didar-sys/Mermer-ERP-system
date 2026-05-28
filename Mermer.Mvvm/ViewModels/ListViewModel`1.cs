using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Data.Authorizers;
using Mermer.Data.Models;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mermer.Mvvm.ViewModels;

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
        Repository = repository;
        Authorizer = authorizer;
    }

    public void Prepare(string parameter) => ItemId = parameter;

    protected override async Task OnLoad()
    {
        List = await Repository.GetAsync();

        if (string.IsNullOrEmpty(ItemId))
            return;

        // Виправлено кашу декомпілятора з лямбдою (\u003COnLoad\u003Eb__5_0)
        SelectedItem = List.SingleOrDefault(x => x.Id == ItemId);
    }

    public bool HasCreateAccess => Authorizer.CanCreate();

    public ICommand CreateNewCommand => new MvxAsyncCommand(OnCreateNewAsync, () => !IsBusy && HasCreateAccess);

    protected virtual Task OnCreateNewAsync()
    {
        return NavigationService.Navigate<DetailsViewModel<T>, string>(string.Empty);
    }

    public ICommand ViewDetailsCommand => new MvxAsyncCommand(OnViewDetailsAsync, () => !IsBusy && SelectedItem != null);

    protected virtual Task OnViewDetailsAsync()
    {
        return NavigationService.Navigate<DetailsViewModel<T>, string>(SelectedItem.Id);
    }

    public ICommand SelectOrViewDetailsCommand => new MvxAsyncCommand(OnSelectOrViewDetailsAsync, () => !IsBusy && SelectedItem != null);

    protected virtual Task OnSelectOrViewDetailsAsync()
    {
        if (!string.IsNullOrEmpty(ItemId))
            return NavigationService.Close(this, SelectedItem.Id);

        ViewDetailsCommand.Execute(null);
        return Task.CompletedTask;
    }
}