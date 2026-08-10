using Mermer.Authorization.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Enterprise.Models;
using Mermer.Finance.Models;
using Mermer.FundsManagement.Models;
using Mermer.Mvvm.Services;
using Mermer.Services;
using Mermer.Transactions.Models;
using Mermer.Transactions.Services;
using Mermer.Ui.Core.Helpers;
using Mermer.Ui.Core.ViewModels.Transactions;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Finance;

public class FundsSlipDetailsViewModel :
  FundsTransactionDetailsViewModel<FundsSlip, FundsSlipLine, FundsSlipType>,
  IMvxViewModel<FundsSlipType>,
  IMvxViewModel
{
    private FundsSlipType _newSlipType;

    public FundsSlipDetailsViewModel(
        IConfigurator configurator,
        ILoginService loginService,
        Reference<Currency> currencies,
        IRepository<FundsSlip> repository,
        IListAuthorizer<FundsSlip> authorizer,
        Reference<Depository> depositories,
        IMvxNavigationService navigationService,
        ITransactionCodeGenerationService codegentor,
        IUserInteractionService userInteractionService)
        : base(repository, authorizer, configurator, loginService, currencies, depositories, navigationService, codegentor, userInteractionService)
    {
    }

    public void Prepare(FundsSlipType parameter) => this._newSlipType = parameter;

    protected override async Task PreLoad()
    {
        await base.PreLoad();

        // Восстанавливаем типы операций для ComboBox
        this.Types = Enum.GetValues(typeof(FundsSlipType))
            .Cast<FundsSlipType>()
            .Select(x => new ListHelper<FundsSlipType>
            {
                Text = this[x.ToString()],
                Value = x
            }).ToArray();

        this.CanChangeType = true;
    }

    protected override async Task PostLoad()
    {
        await base.PostLoad();

        // Загрузка касс напрямую через зарегистрированный в IoC репозиторий IRepository<Depository>
        if (this.Depositories.List == null || !this.Depositories.List.Any())
        {
            // Прямая замена коллекции Depositories.List для UI
            try
            {
                if (Mvx.CanResolve<IRepository<Depository>>())
                {
                    var repo = Mvx.Resolve<IRepository<Depository>>();
                    var list = await repo.GetAsync();
                    if (list != null && list.Any())
                    {
                        // Отключаем фильтрацию базового класса
                        this.Depositories.SuspendLoading = true;
                        this.Depositories.Filter = null;
                        this.Depositories.List = list.ToList();

                        if (Details != null && string.IsNullOrEmpty(Details.DepositoryId))
                        {
                            Details.DepositoryId = list.FirstOrDefault()?.Id;
                        }
                    }
                }
            }
            catch { }
        }

        if (Details != null)
        {
            if (string.IsNullOrEmpty(Details.Code))
                Details.Code = $"FS-{DateTime.Now:yyMMddHHmmss}";

            // Выбор первой кассы по умолчанию
            if (string.IsNullOrEmpty(Details.DepositoryId) && Depositories?.List != null)
            {
                Details.DepositoryId = Depositories.List.FirstOrDefault()?.Id;
            }

            // Настройка конвертации для пересчета Total
            if (Details.CurrencyConvertions == null)
            {
                Details.CurrencyConvertions = new Mermer.Data.WatchedObservableCollection<CurrencyConvertion>();
            }

            if (Currencies?.List != null)
            {
                foreach (var curr in Currencies.List)
                {
                    if (!Details.CurrencyConvertions.Any(c => c.CurrencyId == curr.Id))
                    {
                        Details.CurrencyConvertions.Add(new CurrencyConvertion
                        {
                            Id = Guid.NewGuid().ToString(),
                            CurrencyId = curr.Id,
                            Multiplier = 1,
                            Divider = 1
                        });
                    }
                }

                if (string.IsNullOrEmpty(Details.DisplayCurrencyId))
                    Details.DisplayCurrencyId = Currencies.List.FirstOrDefault()?.Id;
            }

            if (string.IsNullOrEmpty(ItemId))
            {
                Details.SlipType = _newSlipType;
            }

            RaisePropertyChanged(() => Details);
            RaisePropertyChanged(() => Types);
            Depositories?.RaisePropertyChanged("List");
        }
    }

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Details.DepositoryId))
                throw new Exception(this["Field '{0}' is required", this["Depository"]]);

            if (Details.Lines == null || !Details.Lines.Any())
                throw new Exception(this["Document cannot be empty"]);

            foreach (var line in Details.Lines)
            {
                if (line.Amount <= 0)
                    throw new Exception(this["Amount must be greater than zero"]);

                if (string.IsNullOrEmpty(line.CurrencyId))
                    throw new Exception(this["Field '{0}' is required", this["Currency"]]);
            }
        }
        catch (Exception ex)
        {
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        return await base.OnSaveAsync();
    }
}