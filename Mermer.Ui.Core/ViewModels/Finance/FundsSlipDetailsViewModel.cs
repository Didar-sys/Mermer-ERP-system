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
            // 1. Обязательно должна быть выбрана Касса (Depository)
            if (string.IsNullOrEmpty(Details.DepositoryId))
            {
                throw new Exception(this["Field '{0}' is required", this["Depository"]]);
            }

            // 2. Документ не может быть пустым
            if (Details.Lines == null || !Details.Lines.Any())
            {
                throw new Exception(this["Document cannot be empty"]);
            }

            // 3. Проверка каждой строки в таблице
            foreach (var line in Details.Lines)
            {
                // Сумма должна быть больше нуля
                if (line.Amount <= 0)
                    throw new Exception(this["Amount must be greater than zero"]);

                // Валюта обязательна
                if (string.IsNullOrEmpty(line.CurrencyId))
                    throw new Exception(this["Field '{0}' is required", this["Currency"]]);
            }
        }
        catch (Exception ex)
        {
            // Выводим сообщение и блокируем сохранение
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        // Если всё заполнено правильно — вызываем стандартное сохранение
        return await base.OnSaveAsync();
    }

    protected override void Details_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.Details_PropertyChanged(sender, e);

        // Если пользователь изменил главную валюту документа
        if (e.PropertyName == "DisplayCurrencyId")
        {
            // 1. Больше НЕ перезаписываем line.CurrencyId в строках!

            // 2. Делаем микро-паузу, чтобы ядро успело подтянуть правильный курс из базы
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(150);

                // Возвращаемся в главный поток интерфейса для обновления UI
                InvokeOnMainThread(() =>
                {
                    if (this.Details.Lines != null)
                    {
                        foreach (var line in this.Details.Lines)
                        {
                            // Заставляем существующие строки пересчитать свои суммы для отображения
                            line.RaisePropertyChanged("DisplayAmount");
                            line.RaisePropertyChanged("DisplayTotal");
                        }
                    }

                    // Принудительно обновляем итог всего документа и таблицу внизу
                    this.Details.RaisePropertyChanged("DisplayAmount");
                    this.Details.RaisePropertyChanged("DisplayTotal");
                    this.Details.RaisePropertyChanged("Lines");
                });
            });
        }
    }
}
