// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Finance.FundsSlipDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Mermer.Authorization.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.Finance.Models;
using Mermer.FundsManagement.Models;
using Mermer.Mvvm.Services;
using Mermer.Services;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Transactions;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Finance;

public class FundsSlipDetailsViewModel(
  IConfigurator configurator,
  ILoginService loginService,
  Reference<Currency> currencies,
  IRepository<FundsSlip> repository,
  IListAuthorizer<FundsSlip> authorizer,
  Reference<Depository> depositories,
  IMvxNavigationService navigationService,
  ITransactionCodeGenerationService codegentor,
  IUserInteractionService userInteractionService) : 
  FundsTransactionDetailsViewModel<FundsSlip, FundsSlipLine, FundsSlipType>(repository, authorizer, configurator, loginService, currencies, depositories, navigationService, codegentor, userInteractionService),
  IMvxViewModel<FundsSlipType>,
  IMvxViewModel
{
  private FundsSlipType _newSlipType;

  public void Prepare(FundsSlipType parameter) => this._newSlipType = parameter;

    protected override async Task PostLoad()
    {
        await base.PostLoad();

        if (!string.IsNullOrEmpty(ItemId))
            return;

        Details.SlipType = _newSlipType;
    }

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            // 1. Обов'язково має бути вибрана Каса (Depository)
            if (string.IsNullOrEmpty(Details.DepositoryId))
            {
                throw new Exception(this["Field '{0}' is required", this["Depository"]]);
            }

            // 2. Документ не може бути порожнім
            if (Details.Lines == null || !Details.Lines.Any())
            {
                throw new Exception(this["Document cannot be empty"]);
            }

            // 3. Перевірка кожного рядка в таблиці
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
        base.Details_PropertyChanged(sender, e);

        // Якщо користувач змінив головну валюту документа
        if (e.PropertyName == "DisplayCurrencyId")
        {
            // 1. Більше НЕ перезаписуємо line.CurrencyId у рядках!

            // 2. Робимо мікро-паузу, щоб ядро встигло підтягнути правильний курс з бази
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(150);

                // Повертаємось у головний потік інтерфейсу для оновлення UI
                InvokeOnMainThread(() =>
                {
                    if (this.Details.Lines != null)
                    {
                        foreach (var line in this.Details.Lines)
                        {
                            // Змушуємо існуючі рядки перерахувати свої суми для відображення
                            line.RaisePropertyChanged("DisplayAmount");
                            line.RaisePropertyChanged("DisplayTotal");
                        }
                    }

                    // Примусово оновлюємо підсумок усього документа та таблицю внизу
                    this.Details.RaisePropertyChanged("DisplayAmount");
                    this.Details.RaisePropertyChanged("DisplayTotal");
                    this.Details.RaisePropertyChanged("Lines");
                });
            });
        }
    }
}
