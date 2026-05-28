// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Common.TransactionDetailsViewModel`1
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using Mermer.Authorization.Enums;
using Mermer.Authorization.Services;
using Mermer.Transactions.Models;
using Mermer.Transactions.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Models;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Common;

public class TransactionDetailsViewModel<T> : DetailsViewModel<T> where T : class, ITransactionModel, IModel, INotifyPropertyChanged
{
  protected readonly ILoginService LoginService;
  protected readonly ITransactionCodeGenerationService CodeGenerationService;

  public TransactionDetailsViewModel(
    ITransactionCodeGenerationService codeGentor,
    IRepository<T> repository,
    IListAuthorizer<T> authorizer,
    ILoginService loginService,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(repository, authorizer, navigationService, userInteractionService)
  {
    this.LoginService = loginService;
    this.CodeGenerationService = codeGentor;
  }

    protected override async Task OnLoad()
    {
        await base.OnLoad();

        if (!string.IsNullOrEmpty(ItemId))
            return;

        Details.Date = DateTime.Now;
        Details.UserId = LoginService.Session.UserId;
        Details.UserName = LoginService.Session.Username;
        Details.Code = await CodeGenerationService.GetNextCode();

        string name = typeof(T).Name;
        if (name.EndsWith("Order") || name.EndsWith("Revision"))
            return;

        try
        {
            Authorizer.Authorize(Mermer.Authorization.Enums.TransactionAccessLevel.CompleteOwn);
            Details.IsCompleted = true;
        }
        catch (Exception)
        {
            // Ignored
        }
    }
}
