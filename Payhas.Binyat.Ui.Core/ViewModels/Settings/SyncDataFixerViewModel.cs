// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Ui.Core.ViewModels.Settings.SyncDataFixerViewModel
// Assembly: Payhas.Binyat.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Ui.Core.dll

using Autofac;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Payhas.Binyat.Authorization.Models;
using Payhas.Binyat.Commerce.Models;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.CRM.Models;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.Finance.DailyRegistery.Models;
using Payhas.Binyat.Finance.Models;
using Payhas.Binyat.Finance.Spending.Models;
using Payhas.Binyat.FundsManagement.Models;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Warehousing.Models;
using Payhas.Binyat.Warehousing.Ordering.Models;
using Payhas.Binyat.Warehousing.Revisioning.Models;
using Payhas.Data.Models;
using Payhas.Data.Storage;
using Payhas.Mvvm.Services;
using Payhas.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Payhas.Binyat.Ui.Core.ViewModels.Settings;

public class SyncDataFixerViewModel : DialogViewModel
{
  private readonly ICouchCluster _cluster;
  private readonly ILifetimeScope _lifetimeScope;
  private bool _checkMain;
  private bool _checkStocks;
  private bool _checkInvoiceTransactions;
  private bool _checkBillTransactions;
  private bool _checkStockTransactions;
  private bool _checkStockRevisions;
  private bool _checkOtherTransactions;
  private DateTime? _transactionStartDate = new DateTime?(DateTime.Today);
  private DateTime? _transactionEndDate;

  public SyncDataFixerViewModel(
    ICouchCluster cluster,
    IMvxMessenger messenger,
    ILifetimeScope lifetimeScope,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._cluster = cluster;
    this._lifetimeScope = lifetimeScope;
  }

  public bool CheckMain
  {
    get => this._checkMain;
    set => this.SetProperty<bool>(ref this._checkMain, value, nameof (CheckMain));
  }

  public bool CheckStocks
  {
    get => this._checkStocks;
    set => this.SetProperty<bool>(ref this._checkStocks, value, nameof (CheckStocks));
  }

  public bool CheckInvoiceTransactions
  {
    get => this._checkInvoiceTransactions;
    set
    {
      this.SetProperty<bool>(ref this._checkInvoiceTransactions, value, nameof (CheckInvoiceTransactions));
    }
  }

  public bool CheckBillTransactions
  {
    get => this._checkBillTransactions;
    set
    {
      this.SetProperty<bool>(ref this._checkBillTransactions, value, nameof (CheckBillTransactions));
    }
  }

  public bool CheckStockTransactions
  {
    get => this._checkStockTransactions;
    set
    {
      this.SetProperty<bool>(ref this._checkStockTransactions, value, nameof (CheckStockTransactions));
    }
  }

  public bool CheckStockRevisions
  {
    get => this._checkStockRevisions;
    set
    {
      this.SetProperty<bool>(ref this._checkStockRevisions, value, nameof (CheckStockRevisions));
    }
  }

  public bool CheckOtherTransactions
  {
    get => this._checkOtherTransactions;
    set
    {
      this.SetProperty<bool>(ref this._checkOtherTransactions, value, nameof (CheckOtherTransactions));
    }
  }

  public DateTime? TransactionStartDate
  {
    get => this._transactionStartDate;
    set
    {
      this.SetProperty<DateTime?>(ref this._transactionStartDate, value, nameof (TransactionStartDate));
    }
  }

  public DateTime? TransactionEndDate
  {
    get => this._transactionEndDate;
    set
    {
      this.SetProperty<DateTime?>(ref this._transactionEndDate, value, nameof (TransactionEndDate));
    }
  }

  public ICommand CheckCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCheckAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnCheckAsync()
  {
    SyncDataFixerViewModel dataFixerViewModel = this;
    dataFixerViewModel.IsBusy = true;
    try
    {
      using (IBucket bucket = dataFixerViewModel._cluster.OpenDefaultBucket())
      {
        Dictionary<string, Type[]> order = new Dictionary<string, Type[]>()
        {
          {
            "Main",
            new Type[8]
            {
              typeof (Role),
              typeof (User),
              typeof (Currency),
              typeof (Office),
              typeof (Warehouse),
              typeof (Depository),
              typeof (Partner),
              typeof (Expense)
            }
          },
          {
            "Stocks",
            new Type[3]
            {
              typeof (Stock),
              typeof (StockAlternative),
              typeof (StockNameComposer)
            }
          },
          {
            "Invoices",
            new Type[1]{ typeof (Invoice) }
          },
          {
            "Bills",
            new Type[1]{ typeof (Bill) }
          },
          {
            "StockTransactions",
            new Type[2]{ typeof (StockSlip), typeof (StockTransfer) }
          },
          {
            "StockRevisions",
            new Type[1]{ typeof (StockRevision) }
          },
          {
            "OtherTransactions",
            new Type[9]
            {
              typeof (PartnerSlip),
              typeof (PartnerTransfer),
              typeof (FundsSlip),
              typeof (FundsTransfer),
              typeof (ExpenseSlip),
              typeof (DailyFundsRegistery),
              typeof (StockOrder),
              typeof (StockOrderTemplate),
              typeof (AggregatedStockOrder)
            }
          }
        };
        await dataFixerViewModel.CheckInOrder(bucket, order);
      }
    }
    catch (Exception ex)
    {
      dataFixerViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    finally
    {
      dataFixerViewModel.IsBusy = false;
    }
  }

  private async Task CheckInOrder(IBucket bucket, Dictionary<string, Type[]> order)
  {
    SyncDataFixerViewModel dataFixerViewModel = this;
    BucketContext context = new BucketContext(bucket);
    context.EndChangeTracking();
    foreach (KeyValuePair<string, Type[]> keyValuePair in order)
    {
      try
      {
        switch (keyValuePair.Key)
        {
          case "Bills":
            if (dataFixerViewModel.CheckBillTransactions)
              break;
            continue;
          case "Invoices":
            if (dataFixerViewModel.CheckInvoiceTransactions)
              break;
            continue;
          case "Main":
            if (dataFixerViewModel.CheckMain)
              break;
            continue;
          case "OtherTransactions":
            if (dataFixerViewModel.CheckOtherTransactions)
              break;
            continue;
          case "StockRevisions":
            if (dataFixerViewModel.CheckStockRevisions)
              break;
            continue;
          case "StockTransactions":
            if (dataFixerViewModel.CheckStockTransactions)
              break;
            continue;
          case "Stocks":
            if (dataFixerViewModel.CheckStocks)
              break;
            continue;
          default:
            throw new Exception("Unknown check key");
        }
        dataFixerViewModel.Status = $"Starting Checking: {keyValuePair.Key} ...";
        await Task.WhenAll(((IEnumerable<Type>) keyValuePair.Value).Select<Type, Task>((Func<Type, Task>) (x => this.Check(context, x))));
      }
      catch
      {
      }
    }
  }

  public Task Check(BucketContext context, Type type)
  {
    object[] parameters;
    string checkMethodName;
    if (typeof (ITransactionModel).IsAssignableFrom(type))
    {
      checkMethodName = "CheckTransaction";
      parameters = new object[1]{ (object) context };
    }
    else
    {
      checkMethodName = nameof (Check);
      parameters = new object[2]{ (object) context, null };
    }
    return (Task) ((IEnumerable<MethodInfo>) this.GetType().GetMethods()).Single<MethodInfo>((Func<MethodInfo, bool>) (x => x.IsGenericMethod && x.Name == checkMethodName)).MakeGenericMethod(type).Invoke((object) this, parameters);
  }

  public async Task CheckTransaction<T>(BucketContext context) where T : ITransactionModel
  {
    SyncDataFixerViewModel dataFixerViewModel = this;
    IQueryable<T> query = context.Query<T>().Where<T>((Expression<Func<T, bool>>) (x => x.DocType == typeof (T).Name && x.Id == N1QlFunctions.Key((object) x)));
    if (dataFixerViewModel.TransactionStartDate.HasValue)
      query = query.Where<T>((Expression<Func<T, bool>>) (x => (DateTime?) x.Date > dataFixerViewModel.TransactionStartDate));
    if (dataFixerViewModel.TransactionEndDate.HasValue)
      query = query.Where<T>((Expression<Func<T, bool>>) (x => (DateTime?) x.Date > dataFixerViewModel.TransactionEndDate));
    await dataFixerViewModel.Check<T>(context, query);
    if (typeof (T) != typeof (StockRevision))
    {
      query = (IQueryable<T>) null;
    }
    else
    {
      IEnumerable<string> revisionIds = await query.Select<T, string>((Expression<Func<T, string>>) (x => x.Id)).ExecuteAsync<string>();
      IQueryable<StockRevisionLine> query1 = context.Query<StockRevisionLine>().Where<StockRevisionLine>((Expression<Func<StockRevisionLine, bool>>) (x => x.DocType == typeof (StockRevisionLine).Name && x.Id == N1QlFunctions.Key(x) && revisionIds.Contains<string>(x.StockRevisionId)));
      await dataFixerViewModel.Check<StockRevisionLine>(context, query1);
      query = (IQueryable<T>) null;
    }
  }

  public async Task Check<T>(BucketContext context, IQueryable<T> query) where T : IModel
  {
    try
    {
      IQueryable<T> queryable = query;
      if (queryable == null)
        queryable = context.Query<T>().Where<T>((Expression<Func<T, bool>>) (x => x.DocType == typeof (T).Name && x.Id == N1QlFunctions.Key((object) x)));
      query = queryable;
      int total = await ((IQueryable<T>) query).ExecuteAsync<T, int>((Expression<Func<IQueryable<T>, int>>) (q => q.Count<T>()));
      if (total == 0)
        return;
      IRepository<T> writer = this._lifetimeScope.Resolve<IRepository<T>>();
      for (int skip = 0; skip < total; skip += 1024 /*0x0400*/)
      {
        List<T> list = (await ((IQueryable<T>) query).OrderBy<T, string>((Expression<Func<T, string>>) (x => x.Id)).Skip<T>(skip).Take<T>(1024 /*0x0400*/).ExecuteAsync<T>()).ToList<T>();
        if (!list.Any<T>())
          return;
        foreach (T model in list)
        {
          try
          {
            await writer.UpdateAsync(model);
          }
          catch
          {
          }
        }
      }
      writer = (IRepository<T>) null;
    }
    catch
    {
    }
  }
}
