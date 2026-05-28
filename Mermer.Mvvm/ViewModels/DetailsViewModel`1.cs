using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Data.Authorizers;
using Mermer.Data.Models;
using Mermer.Data.Storage;
using Mermer.Data.Tools;
using Mermer.Mvvm.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

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
        Repository = repository;
        Authorizer = authorizer;
    }

    public override string Caption
    {
        get => (_caption ?? this[typeof(T).Name]) + (IsDirty ? " *" : "");
        set => _caption = value;
    }

    public override string SubCaption
    {
        get => _subCaption ?? Details?.ToString();
        set => _subCaption = value;
    }

    public virtual string ItemId
    {
        get => _itemId;
        set => SetProperty(ref _itemId, value);
    }

    public virtual T Details
    {
        get => _details;
        set
        {
            SetProperty(ref _details, value);
            RaisePropertyChanged(() => SubCaption);
        }
    }

    public virtual bool IsDirty
    {
        get => _isDirty;
        set
        {
            SetProperty(ref _isDirty, value);
            RaisePropertyChanged(() => Caption);
            RaisePropertyChanged(() => SubCaption);
        }
    }

    public virtual bool HasSaveAccess
    {
        get => _hasSaveAccess;
        set => SetProperty(ref _hasSaveAccess, value);
    }

    public virtual bool HasCreateAccess
    {
        get => _hasCreateAccess;
        set => SetProperty(ref _hasCreateAccess, value);
    }

    public void Prepare(string parameter) => ItemId = parameter;

    protected override Task PreLoad()
    {
        HasSaveAccess = string.IsNullOrEmpty(ItemId) ? Authorizer.CanCreate() : Authorizer.CanUpdate();
        HasCreateAccess = Authorizer.CanCreate();
        return base.PreLoad();
    }

    public override async Task Initialize()
    {
        await base.Initialize(); // Виправлено кашу декомпілятора (\u003C\u003En__0)
        IsDirty = false;
        if (Details == null)
            return;

        // Виправлено кашу декомпілятора з лямбдою (\u003CInitialize\u003Eb__33_0)
        DirtynessController.ControlDocument<T>(Details, _ => IsDirty = true);
    }

    protected override async Task OnLoad()
    {
        if (!string.IsNullOrEmpty(ItemId))
        {
            Details = await Repository.GetAsync(ItemId);
            if (Details == null)
                throw new Exception(this["Item was not found!"]);
        }
        else
        {
            Details = Activator.CreateInstance<T>();
            Details.Id = Guid.NewGuid().ToString();
        }
    }

    protected virtual bool CanSave() => !IsBusy && IsDirty && HasSaveAccess;

    public ICommand SaveCommand => new MvxAsyncCommand(OnSaveAsync, CanSave);

    protected virtual async Task<bool> OnSaveAsync()
    {
        IsBusy = true;
        bool succeed = false;
        try
        {
            if (string.IsNullOrEmpty(ItemId))
                await Repository.CreateAsync(Details);
            else
                await Repository.UpdateAsync(Details);

            ItemId = Details.Id;
            IsDirty = false;
            succeed = true;
        }
        catch (Exception ex)
        {
            UserInteractionService.ShowExceptionMessage(new Exception(string.Format(this["Error saving {0}", this[typeof(T).Name]]), ex));
        }
        IsBusy = false;
        return succeed;
    }

    public ICommand SaveAndNewCommand => new MvxAsyncCommand(OnSaveAndNewAsync, () => CanSave() && HasCreateAccess);

    private Task OnSaveAndNewAsync()
    {
        return OnSaveAsync().ContinueWith(t =>
        {
            if (!t.Result)
                return Task.CompletedTask;
            ItemId = string.Empty;
            Details = default(T);
            return Initialize();
        });
    }

    public ICommand SaveAndCloseCommand => new MvxAsyncCommand(OnSaveAndCloseAsync, CanSave);

    private Task OnSaveAndCloseAsync()
    {
        return OnSaveAsync().ContinueWith(t => t.Result ? OnCloseAsync() : Task.CompletedTask);
    }

    public ICommand ReloadCommand => new MvxAsyncCommand(OnReloadAsync, () => !IsBusy);

    private Task OnReloadAsync()
    {
        if (IsDirty)
        {
            bool? nullable = UserInteractionService.ShowMessage(this["Reloading"], this["Would you like to save?"], UserInteractionType.YesNoCancel);
            if (!nullable.HasValue)
                return Task.CompletedTask;
            if (nullable.Value)
                return OnSaveAsync().ContinueWith(t => t.Result ? Initialize() : Task.CompletedTask);
        }
        return Initialize();
    }

    public override async Task<bool> OnCloseAsync()
    {
        if (IsDirty)
        {
            bool? nullable = UserInteractionService.ShowMessage(this["Closing"], this["Would you like to save?"], UserInteractionType.YesNoCancel);
            if (!nullable.HasValue)
                return false;
            if (nullable.Value)
            {
                if (!await OnSaveAsync())
                    return false;
            }
        }

        return await base.OnCloseAsync(); // Виправлено кашу декомпілятора (\u003C\u003En__1)
    }
}