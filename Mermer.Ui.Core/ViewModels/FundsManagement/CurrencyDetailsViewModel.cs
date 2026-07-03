// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.FundsManagement.CurrencyDetailsViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Mermer.Data.Authorizers;
using Mermer.Data.Storage;
using Mermer.FundsManagement.Models;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using MvvmCross.Core.Navigation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.FundsManagement;

public class CurrencyDetailsViewModel(
  IRepository<Currency> repository,
  IListAuthorizer<Currency> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : DetailsViewModel<Currency>(repository, authorizer, navigationService, userInteractionService)
{
    protected override async Task PostLoad()
    {
        await base.PostLoad();

        if (Details.Rates == null)
            Details.Rates = new ObservableCollection<CurrencyRate>();

        Details.PropertyChanged += Details_PropertyChanged;
    }

    private void Details_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (!(e.PropertyName == "IsDefault"))
      return;
    if (this.Details.IsDefault)
    {
      Currency details = this.Details;
      ObservableCollection<CurrencyRate> observableCollection = new ObservableCollection<CurrencyRate>();
      observableCollection.Add(new CurrencyRate()
      {
        ValidFrom = DateTime.Today,
        Multiplier = 1M,
        Divider = 1M
      });
      details.Rates = observableCollection;
    }
    else
      this.Details.Rates = new ObservableCollection<CurrencyRate>();
  }

    protected override async Task<bool> OnSaveAsync()
    {
        try
        {
            // Перевіряємо, чи введено назву валюти (наприклад, "USD" або "Гривня")
            if (string.IsNullOrWhiteSpace(Details.Name))
            {
                throw new Exception(this["Field '{0}' is required", this["Name"]]);
            }

            // Додаткова перевірка: якщо валюта НЕ дефолтна, у неї має бути хоча б один курс
            if (!Details.IsDefault && (Details.Rates == null || !Details.Rates.Any()))
            {
                // Цю перевірку можна закоментувати, якщо у вас дозволено створювати валюти без курсів,
                // але для фінансової системи зазвичай курс до базової валюти потрібен завжди.
                // throw new Exception(this["Field '{0}' is required", this["Rates"]]);
            }
        }
        catch (Exception ex)
        {
            // Виводимо повідомлення користувачу
            UserInteractionService.ShowExceptionMessage(ex);

            // Блокуємо збереження
            return false;
        }

        // Якщо все добре — викликаємо базове збереження
        return await base.OnSaveAsync();
    }
}
