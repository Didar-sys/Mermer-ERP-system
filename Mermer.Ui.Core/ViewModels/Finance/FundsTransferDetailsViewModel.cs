// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Finance.FundsTransferDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Mermer.Authorization.Services;
using Mermer.Enterprise.Models;
using Mermer.Finance.Models;
using Mermer.FundsManagement.Models;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Transactions;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Finance;

public class FundsTransferDetailsViewModel(
  IConfigurator configurator,
  ILoginService loginService,
  Reference<Currency> currencies,
  Reference<Depository> depositories,
  IRepository<FundsTransfer> repository,
  IListAuthorizer<FundsTransfer> authorizer,
  IMvxNavigationService navigationService,
  ITransactionCodeGenerationService codegentor,
  IUserInteractionService userInteractionService) : 
  FundsTransactionDetailsViewModel<FundsTransfer, FundsTransferLine>(repository, authorizer, configurator, loginService, currencies, depositories, navigationService, codegentor, userInteractionService)
{
    protected override async Task PostLoad()
    {
        var usedDepositoryIds = new[]
        {
        Details.DepositoryId,
        Details.DestinationDepositoryId
    }.Distinct();

        await base.PostLoad();

        Depositories.Filter = x => !x.IsDisabled || usedDepositoryIds.Contains(x.Id);
    }

    public ICommand SelectDestinationDepositoryCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnSelectDestinationDepositoryCommandAsync), (Func<bool>) (() => !this.IsBusy && this.HasSaveAccess));
    }
  }

  private async Task OnSelectDestinationDepositoryCommandAsync()
  {
    FundsTransferDetailsViewModel detailsViewModel = this;
    FundsTransfer fundsTransfer = detailsViewModel.Details;
    fundsTransfer.DestinationDepositoryId = await detailsViewModel.NavigationService.Navigate<ListViewModel<Depository>, string, string>(detailsViewModel.Details.DestinationDepositoryId ?? Guid.Empty.ToString());
    fundsTransfer = (FundsTransfer) null;
  }

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            // 1. Обов'язково має бути вказана каса-відправник
            if (string.IsNullOrEmpty(Details.DepositoryId))
            {
                throw new Exception(this["Field '{0}' is required", this["Source Depository"]]);
            }

            // 2. Обов'язково має бути вказана каса-одержувач
            if (string.IsNullOrEmpty(Details.DestinationDepositoryId))
            {
                throw new Exception(this["Field '{0}' is required", this["Destination Depository"]]);
            }

            // 3. Логічний захист: каса-відправник і каса-одержувач не можуть бути однаковими
            if (Details.DepositoryId == Details.DestinationDepositoryId)
            {
                throw new Exception(this["Source and destination depositories cannot be the same"]);
            }

            // 4. Документ не може бути порожнім
            if (Details.Lines == null || !Details.Lines.Any())
            {
                throw new Exception(this["Document cannot be empty"]);
            }

            // 5. Перевірка кожного рядка в таблиці
            foreach (var line in Details.Lines)
            {
                // Сума має бути більшою за нуль
                if (line.Amount <= 0)
                    throw new Exception(this["Amount must be greater than zero"]);

                // Валюта є обов'язковою
                if (string.IsNullOrEmpty(line.CurrencyId))
                    throw new Exception(this["Field '{0}' is required", this["Currency"]]);
            }
        }
        catch (Exception ex)
        {
            // Виводимо повідомлення та блокуємо збереження
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        // Якщо все заповнено правильно — викликаємо стандартне збереження
        return await base.OnSaveAsync();
    }
}
