// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Finance.FundsTransferDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Mermer.Authorization.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.Finance.Models;
using Mermer.FundsManagement.Models;
using Mermer.FundsManagement.Models.Extenders;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.Services;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Transactions;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
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

    protected override void Details_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "DisplayCurrencyId")
        {
            var newCurrencyId = this.Details.DisplayCurrencyId;

            // =========================================================
            // БЛОК ПРАВИЛЬНОЇ КОНВЕРТАЦІЇ ФІНАНСІВ (USD <-> TMT)
            // =========================================================
            if (this.Details.Lines != null && !string.IsNullOrEmpty(newCurrencyId))
            {
                var targetCurrency = this.Currencies?.List?.FirstOrDefault(c => c.Id == newCurrencyId);

                foreach (var line in this.Details.Lines)
                {
                    if (string.IsNullOrEmpty(line.CurrencyId) || line.CurrencyId == newCurrencyId)
                        continue;

                    var sourceCurrency = this.Currencies?.List?.FirstOrDefault(c => c.Id == line.CurrencyId);

                    if (sourceCurrency != null && targetCurrency != null)
                    {
                        var sourceRate = sourceCurrency.GetRate(this.Details.Date);
                        var targetRate = targetCurrency.GetRate(this.Details.Date);

                        if (sourceRate != null && targetRate != null && sourceRate.Divider != 0 && targetRate.Multiplier != 0)
                        {
                            decimal sMult = sourceRate.Multiplier;
                            decimal sDiv = sourceRate.Divider;
                            decimal tMult = targetRate.Multiplier;
                            decimal tDiv = targetRate.Divider;

                            // Вираховуємо загальний коефіцієнт конвертації
                            decimal conversionRate = (sMult / sDiv) * (tDiv / tMult);

                            // Конвертуємо ВІДПРАВЛЕНУ суму
                            line.Amount = Math.Round(line.Amount * conversionRate, targetCurrency.Decimals);

                            // ДОДАНО: Конвертуємо ПРИЙНЯТУ суму
                            line.ReceivedAmount = Math.Round(line.ReceivedAmount * conversionRate, targetCurrency.Decimals);

                            line.CurrencyId = newCurrencyId;
                        }
                    }
                }
            }
            // =========================================================

            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(150);
                InvokeOnMainThread(() =>
                {
                    if (this.Details.Lines != null)
                    {
                        foreach (var line in this.Details.Lines)
                        {
                            line.RaisePropertyChanged("Amount");
                            line.RaisePropertyChanged("DisplayAmount");
                            line.RaisePropertyChanged("DisplayTotal");

                            // ДОДАНО: Сигнал для оновлення колонок переміщення
                            line.RaisePropertyChanged("ActionTotal");
                            line.RaisePropertyChanged("ActionReceivedTotal");
                        }
                    }
                    this.Details.RaisePropertyChanged("DisplayAmount");
                    this.Details.RaisePropertyChanged("DisplayTotal");

                    // ДОДАНО: Сигнал для оновлення загальних підсумків унизу екрана
                    this.Details.RaisePropertyChanged("ActionTotal");
                    this.Details.RaisePropertyChanged("ActionReceivedTotal");

                    this.Details.RaisePropertyChanged("Lines");
                });
            });

            return;
        }

        base.Details_PropertyChanged(sender, e);
    }
}
