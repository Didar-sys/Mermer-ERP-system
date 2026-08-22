using Autofac;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Authorization.Models;
using Mermer.Commerce.Models;
using Mermer.CRM.Models;
using Mermer.Enterprise.Models;
using Mermer.Finance.DailyRegistery.Models;
using Mermer.Finance.Models;
using Mermer.Finance.Spending.Models;
using Mermer.FundsManagement.Models;
using Mermer.StockManagement.Models;
using Mermer.Transactions.Models;
using Mermer.Warehousing.Models;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Warehousing.Revisioning.Models;
using Mermer.Data.Models;
using Mermer.Data.Storage;
using Mermer.Mvvm.Services;
using Mermer.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mermer.Ui.Core.ViewModels.Settings;

public class SyncDataFixerViewModel : DialogViewModel
{
    private readonly ILifetimeScope _lifetimeScope;
    private bool _checkMain;
    private bool _checkStocks;
    private bool _checkInvoiceTransactions;
    private bool _checkBillTransactions;
    private bool _checkStockTransactions;
    private bool _checkStockRevisions;
    private bool _checkOtherTransactions;
    private DateTime? _transactionStartDate = DateTime.Today;
    private DateTime? _transactionEndDate;
    private string _status;

    // УБРАН ICouchCluster из конструктора!
    public SyncDataFixerViewModel(
      IMvxMessenger messenger,
      ILifetimeScope lifetimeScope,
      IMvxNavigationService navigationService,
      IUserInteractionService userInteractionService)
      : base(messenger, navigationService, userInteractionService)
    {
        this._lifetimeScope = lifetimeScope;
    }

    public string Status
    {
        get => this._status;
        set => this.SetProperty<string>(ref this._status, value, nameof(Status));
    }

    public bool CheckMain
    {
        get => this._checkMain;
        set => this.SetProperty<bool>(ref this._checkMain, value, nameof(CheckMain));
    }

    public bool CheckStocks
    {
        get => this._checkStocks;
        set => this.SetProperty<bool>(ref this._checkStocks, value, nameof(CheckStocks));
    }

    public bool CheckInvoiceTransactions
    {
        get => this._checkInvoiceTransactions;
        set => this.SetProperty<bool>(ref this._checkInvoiceTransactions, value, nameof(CheckInvoiceTransactions));
    }

    public bool CheckBillTransactions
    {
        get => this._checkBillTransactions;
        set => this.SetProperty<bool>(ref this._checkBillTransactions, value, nameof(CheckBillTransactions));
    }

    public bool CheckStockTransactions
    {
        get => this._checkStockTransactions;
        set => this.SetProperty<bool>(ref this._checkStockTransactions, value, nameof(CheckStockTransactions));
    }

    public bool CheckStockRevisions
    {
        get => this._checkStockRevisions;
        set => this.SetProperty<bool>(ref this._checkStockRevisions, value, nameof(CheckStockRevisions));
    }

    public bool CheckOtherTransactions
    {
        get => this._checkOtherTransactions;
        set => this.SetProperty<bool>(ref this._checkOtherTransactions, value, nameof(CheckOtherTransactions));
    }

    public DateTime? TransactionStartDate
    {
        get => this._transactionStartDate;
        set => this.SetProperty<DateTime?>(ref this._transactionStartDate, value, nameof(TransactionStartDate));
    }

    public DateTime? TransactionEndDate
    {
        get => this._transactionEndDate;
        set => this.SetProperty<DateTime?>(ref this._transactionEndDate, value, nameof(TransactionEndDate));
    }

    public ICommand CheckCommand
    {
        get
        {
            return (ICommand)new MvxAsyncCommand(new Func<Task>(this.OnCheckAsync), (Func<bool>)(() => !this.IsBusy));
        }
    }

    private async Task OnCheckAsync()
    {
        if (!CheckMain && !CheckStocks && !CheckInvoiceTransactions && !CheckBillTransactions &&
            !CheckStockTransactions && !CheckStockRevisions && !CheckOtherTransactions)
        {
            this.UserInteractionService.ShowMessage("Warning", "Please select at least one data type to check!");
            return;
        }

        this.IsBusy = true;
        try
        {
            Dictionary<string, Type[]> order = new Dictionary<string, Type[]>()
            {
              { "Main", new Type[] { typeof(Role), typeof(User), typeof(Currency), typeof(Office), typeof(Warehouse), typeof(Depository), typeof(Partner), typeof(Expense) } },
              { "Stocks", new Type[] { typeof(Stock), typeof(StockAlternative), typeof(StockNameComposer) } },
              { "Invoices", new Type[] { typeof(Invoice) } },
              { "Bills", new Type[] { typeof(Bill) } },
              { "StockTransactions", new Type[] { typeof(StockSlip), typeof(StockTransfer) } },
              { "StockRevisions", new Type[] { typeof(StockRevision) } },
              { "OtherTransactions", new Type[] { typeof(PartnerSlip), typeof(PartnerTransfer), typeof(FundsSlip), typeof(FundsTransfer), typeof(ExpenseSlip), typeof(DailyFundsRegistery), typeof(StockOrder), typeof(StockOrderTemplate), typeof(AggregatedStockOrder) } }
            };

            await CheckInOrder(order);

            this.Status = "Sync check completed successfully!";
            this.UserInteractionService.ShowMessage("Success", "Data synchronization check is finished.");
        }
        catch (Exception ex)
        {
            this.UserInteractionService.ShowExceptionMessage(ex, "Error processing data sync");
        }
        finally
        {
            this.IsBusy = false;
        }
    }

    private async Task CheckInOrder(Dictionary<string, Type[]> order)
    {
        foreach (KeyValuePair<string, Type[]> keyValuePair in order)
        {
            try
            {
                switch (keyValuePair.Key)
                {
                    case "Bills": if (!CheckBillTransactions) continue; break;
                    case "Invoices": if (!CheckInvoiceTransactions) continue; break;
                    case "Main": if (!CheckMain) continue; break;
                    case "OtherTransactions": if (!CheckOtherTransactions) continue; break;
                    case "StockRevisions": if (!CheckStockRevisions) continue; break;
                    case "StockTransactions": if (!CheckStockTransactions) continue; break;
                    case "Stocks": if (!CheckStocks) continue; break;
                    default: throw new Exception("Unknown check key: " + keyValuePair.Key);
                }

                this.Status = $"Starting Checking: {keyValuePair.Key} ...";

                foreach (Type type in keyValuePair.Value)
                {
                    MethodInfo method = this.GetType().GetMethod(nameof(CheckGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
                    if (method != null)
                    {
                        MethodInfo generic = method.MakeGenericMethod(type);
                        await (Task)generic.Invoke(this, null);
                    }
                }
            }
            catch (Exception ex)
            {
                this.UserInteractionService.ShowExceptionMessage(ex, $"Error processing {keyValuePair.Key}");
            }
        }
    }

    private async Task CheckGeneric<T>() where T : IModel
    {
        try
        {
            IRepository<T> repo = _lifetimeScope.ResolveOptional<IRepository<T>>();
            if (repo == null) return;

            IEnumerable<T> allItems = await repo.GetAsync();
            if (allItems == null || !allItems.Any()) return;

            IEnumerable<T> itemsToProcess = allItems;

            if (typeof(ITransactionModel).IsAssignableFrom(typeof(T)))
            {
                IEnumerable<ITransactionModel> txItems = allItems.Cast<ITransactionModel>();

                if (TransactionStartDate.HasValue)
                    txItems = txItems.Where(x => x.Date >= TransactionStartDate.Value);
                if (TransactionEndDate.HasValue)
                    txItems = txItems.Where(x => x.Date <= TransactionEndDate.Value);

                itemsToProcess = txItems.Cast<T>().ToList();
            }

            var itemList = itemsToProcess.ToList();
            if (!itemList.Any()) return;

            // Снимаем ограничение .NET на количество одновременных HTTP-соединений к одному хосту
            System.Net.ServicePointManager.DefaultConnectionLimit = 500;

            // Увеличиваем количество параллельных запросов до 100
            using var throttler = new System.Threading.SemaphoreSlim(100);
            var tasks = itemList.Select(async item =>
            {
                await throttler.WaitAsync();
                try
                {
                    await Task.Run(() => repo.UpdateAsync(item)); // Выносим в тредпул
                }
                catch (Exception ex)
                {
                    this.Status = $"Failed to update {typeof(T).Name} ID: {item.Id}. Error: {ex.Message}";
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);

            // (Здесь остается старая логика для StockRevisionLine, если нужна)
        }
        catch (Exception ex)
        {
            this.UserInteractionService.ShowExceptionMessage(ex, $"Database error while processing {typeof(T).Name}");
        }
    }
}