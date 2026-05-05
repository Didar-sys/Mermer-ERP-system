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
    TransactionDetailsViewModel<T> detailsViewModel = this;
    // ISSUE: reference to a compiler-generated method
    await detailsViewModel.\u003C\u003En__0();
    if (!string.IsNullOrEmpty(detailsViewModel.ItemId))
      return;
    detailsViewModel.Details.Date = DateTime.Now;
    detailsViewModel.Details.UserId = detailsViewModel.LoginService.Session.UserId;
    detailsViewModel.Details.UserName = detailsViewModel.LoginService.Session.Username;
    T obj = detailsViewModel.Details;
    obj.Code = await detailsViewModel.CodeGenerationService.GetNextCode();
    obj = default (T);
    string name = typeof (T).Name;
    if (name.EndsWith("Order"))
      return;
    if (name.EndsWith("Revision"))
      return;
    try
    {
      detailsViewModel.Authorizer.Authorize((Enum) TransactionAccessLevel.CompleteOwn);
      detailsViewModel.Details.IsCompleted = true;
    }
    catch (Exception ex)
    {
    }
  }
}
