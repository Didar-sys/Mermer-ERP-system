// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Finance.Spending.ExpensesListViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Finance.Spending.Models;
using Payhas.Binyat.Ui.Core.ViewModels.Common;
using Payhas.Data.Authorizers;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Finance.Spending;

public class ExpensesListViewModel(
  IMvxMessenger messenger,
  IRepository<Expense> repository,
  IListAuthorizer<Expense> authorizer,
  IMvxNavigationService navigationService,
  IUserInteractionService userInteractionService) : ListViewModel<Expense>(repository, authorizer, messenger, navigationService, userInteractionService)
{
  public ICommand ImportCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnImportCommandAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  protected virtual async Task OnImportCommandAsync()
  {
    ExpensesListViewModel expensesListViewModel = this;
    IEnumerable<object> source1 = await expensesListViewModel.NavigationService.Navigate<DataImportViewModel, Type, IEnumerable<object>>(typeof (ExpensesListViewModel.ExpenseImport));
    int i = 0;
    expensesListViewModel.IsBusy = true;
    expensesListViewModel.SuspendLoading = true;
    try
    {
      IEnumerable<ExpensesListViewModel.ExpenseImport> source2 = source1 != null ? source1.Cast<ExpensesListViewModel.ExpenseImport>() : (IEnumerable<ExpensesListViewModel.ExpenseImport>) null;
      if (source2 != null)
      {
        int itemsCount = source2.Count<ExpensesListViewModel.ExpenseImport>();
        foreach (ExpensesListViewModel.ExpenseImport expenseImport in source2)
        {
          ++i;
          expensesListViewModel.Status = expensesListViewModel["Importing {0} of {1} items", new object[2]
          {
            (object) i,
            (object) itemsCount
          }];
          Expense expense = new Expense();
          expense.Id = Guid.NewGuid().ToString();
          expense.Name = expenseImport.Name;
          expense.Group = expenseImport.Group;
          expense.Type = expenseImport.Type;
          Expense model = expense;
          await expensesListViewModel.Repository.CreateAsync(model);
        }
      }
    }
    catch (Exception ex)
    {
      expensesListViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    expensesListViewModel.Status = (string) null;
    expensesListViewModel.SuspendLoading = false;
    expensesListViewModel.IsBusy = false;
    expensesListViewModel.ReloadCommand.Execute((object) null);
  }

  public class ExpenseImport
  {
    public string Name { get; internal set; }

    public string Group { get; internal set; }

    public string Type { get; internal set; }
  }
}
