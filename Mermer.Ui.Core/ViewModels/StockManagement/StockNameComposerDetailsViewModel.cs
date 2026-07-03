using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using Mermer.StockManagement.Models;
using MvvmCross.Core.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.StockManagement;

public class StockNameComposerDetailsViewModel(
  IRepository<StockNameComposer> repository,
  IListAuthorizer<StockNameComposer> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : DetailsViewModel<StockNameComposer>(repository, authorizer, navigationService, userInteractionService)
{
    protected override async Task PostLoad()
    {
        await base.PostLoad();

        if (Details.Values == null)
            Details.Values = new ObservableCollection<StockNameComposerValue>();
    }

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            // 1. Обов'язкова перевірка поля "Назва" (Name)
            if (string.IsNullOrEmpty(Details.Name))
            {
                throw new Exception(this["Field '{0}' is required", this["Name"]]);
            }

            // 2. Заборона створення порожнього конструктора без значень
            if (Details.Values == null || !Details.Values.Any())
            {
                throw new Exception(this["Document cannot be empty"]);
            }

            // 3. Тепер ми можемо легально і прямо записати унікальний ідентифікатор!
            foreach (var valueItem in Details.Values)
            {
                if (string.IsNullOrEmpty(valueItem.Id))
                {
                    valueItem.Id = Guid.NewGuid().ToString();
                }
            }
        }
        catch (Exception ex)
        {
            UserInteractionService.ShowExceptionMessage(ex);
            return false;
        }

        // Стандартне і безпечне збереження
        return await base.OnSaveAsync();
    }
}