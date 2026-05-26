// Decompiled with JetBrains decompiler
// Type: Mermer.Ui.Core.ViewModels.Settings.CouchDataCopierViewModel
// Assembly: Mermer.Ui.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DC92D011-8413-44AC-9F10-F866D891CF66
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Ui.Core.dll

using Autofac;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using Mermer.Authorization.Models;
using Mermer.Commerce.Models;
using Mermer.Core.Couch.Common;
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
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

#nullable disable
namespace Mermer.Ui.Core.ViewModels.Settings;

public class CouchDataCopierViewModel : DialogViewModel
{
  private readonly ILifetimeScope _lifetimeScope;
  private string _sourceUrl = "http://192.168.1.?:8091";
  private string _sourceBucket = "binyat.ymb3";
  private string _sourceUsername = "admin";
  private string _sourcePassword = "PwdAdm321";
  private bool _copyMain;
  private bool _copyStocks;
  private bool _copyInvoiceTransactions;
  private bool _copyBillTransactions;
  private bool _copyStockTransactions;
  private bool _copyStockRevisions;
  private bool _copyOtherTransactions;
  private DateTime? _transactionStartDate = new DateTime?(DateTime.Parse("2019-01-01"));
  private DateTime? _transactionEndDate;

  public CouchDataCopierViewModel(
    IMvxMessenger messenger,
    ILifetimeScope lifetimeScope,
    IMvxNavigationService navigationService,
    IUserInteractionService userInteractionService)
    : base(messenger, navigationService, userInteractionService)
  {
    this._lifetimeScope = lifetimeScope;
  }

  public string SourceUrl
  {
    get => this._sourceUrl;
    set => this.SetProperty<string>(ref this._sourceUrl, value, nameof (SourceUrl));
  }

  public string SourceBucket
  {
    get => this._sourceBucket;
    set => this.SetProperty<string>(ref this._sourceBucket, value, nameof (SourceBucket));
  }

  public string SourceUsername
  {
    get => this._sourceUsername;
    set => this.SetProperty<string>(ref this._sourceUsername, value, nameof (SourceUsername));
  }

  public string SourcePassword
  {
    get => this._sourcePassword;
    set => this.SetProperty<string>(ref this._sourcePassword, value, nameof (SourcePassword));
  }

  public bool CopyMain
  {
    get => this._copyMain;
    set => this.SetProperty<bool>(ref this._copyMain, value, nameof (CopyMain));
  }

  public bool CopyStocks
  {
    get => this._copyStocks;
    set => this.SetProperty<bool>(ref this._copyStocks, value, nameof (CopyStocks));
  }

  public bool CopyInvoiceTransactions
  {
    get => this._copyInvoiceTransactions;
    set
    {
      this.SetProperty<bool>(ref this._copyInvoiceTransactions, value, nameof (CopyInvoiceTransactions));
    }
  }

  public bool CopyBillTransactions
  {
    get => this._copyBillTransactions;
    set
    {
      this.SetProperty<bool>(ref this._copyBillTransactions, value, nameof (CopyBillTransactions));
    }
  }

  public bool CopyStockTransactions
  {
    get => this._copyStockTransactions;
    set
    {
      this.SetProperty<bool>(ref this._copyStockTransactions, value, nameof (CopyStockTransactions));
    }
  }

  public bool CopyStockRevisions
  {
    get => this._copyStockRevisions;
    set => this.SetProperty<bool>(ref this._copyStockRevisions, value, nameof (CopyStockRevisions));
  }

  public bool CopyOtherTransactions
  {
    get => this._copyOtherTransactions;
    set
    {
      this.SetProperty<bool>(ref this._copyOtherTransactions, value, nameof (CopyOtherTransactions));
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

  public ICommand CopyCommand
  {
    get
    {
      return (ICommand) new MvxAsyncCommand(new Func<Task>(this.OnCopyAsync), (Func<bool>) (() => !this.IsBusy));
    }
  }

  private async Task OnCopyAsync()
  {
    CouchDataCopierViewModel dataCopierViewModel = this;
    dataCopierViewModel.IsBusy = true;
    try
    {
      CouchCluster couchCluster = new CouchCluster();
      couchCluster.Initialize(dataCopierViewModel.SourceUrl, dataCopierViewModel.SourceBucket, dataCopierViewModel.SourceUsername, dataCopierViewModel.SourcePassword);
      using (IBucket bucket = couchCluster.OpenDefaultBucket())
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
        await dataCopierViewModel.CopyInOrder(bucket, order);
      }
    }
    catch (Exception ex)
    {
      dataCopierViewModel.UserInteractionService.ShowExceptionMessage(ex);
    }
    finally
    {
      dataCopierViewModel.IsBusy = false;
    }
  }

  private async Task CopyInOrder(IBucket bucket, Dictionary<string, Type[]> order)
  {
    CouchDataCopierViewModel dataCopierViewModel = this;
    BucketContext context = new BucketContext(bucket);
    context.EndChangeTracking();
    foreach (KeyValuePair<string, Type[]> keyValuePair in order)
    {
      try
      {
        switch (keyValuePair.Key)
        {
          case "Bills":
            if (dataCopierViewModel.CopyBillTransactions)
              break;
            continue;
          case "Invoices":
            if (dataCopierViewModel.CopyInvoiceTransactions)
              break;
            continue;
          case "Main":
            if (dataCopierViewModel.CopyMain)
              break;
            continue;
          case "OtherTransactions":
            if (dataCopierViewModel.CopyOtherTransactions)
              break;
            continue;
          case "StockRevisions":
            if (dataCopierViewModel.CopyStockRevisions)
              break;
            continue;
          case "StockTransactions":
            if (dataCopierViewModel.CopyStockTransactions)
              break;
            continue;
          case "Stocks":
            if (dataCopierViewModel.CopyStocks)
              break;
            continue;
          default:
            throw new Exception("Unknown copy key");
        }
        dataCopierViewModel.Status = $"Starting Copying: {keyValuePair.Key} ...";
        await Task.WhenAll(((IEnumerable<Type>) keyValuePair.Value).Select<Type, Task>((Func<Type, Task>) (x => this.Copy(context, x))));
      }
      catch
      {
      }
    }
  }

  public Task Copy(BucketContext context, Type type)
  {
    object[] parameters;
    string copyMethodName;
    if (typeof (ITransactionModel).IsAssignableFrom(type))
    {
      copyMethodName = "CopyTransaction";
      parameters = new object[1]{ (object) context };
    }
    else
    {
      copyMethodName = nameof (Copy);
      parameters = new object[2]{ (object) context, null };
    }
    return (Task) ((IEnumerable<MethodInfo>) this.GetType().GetMethods()).Single<MethodInfo>((Func<MethodInfo, bool>) (x => x.IsGenericMethod && x.Name == copyMethodName)).MakeGenericMethod(type).Invoke((object) this, parameters);
  }

  public async Task CopyTransaction<T>(BucketContext context) where T : ITransactionModel
  {
    CouchDataCopierViewModel dataCopierViewModel = this;
    IQueryable<T> query = context.Query<T>().Where<T>((Expression<Func<T, bool>>) (x => x.DocType == typeof (T).Name && x.Id == N1QlFunctions.Key((object) x)));
    if (dataCopierViewModel.TransactionStartDate.HasValue)
      query = query.Where<T>((Expression<Func<T, bool>>) (x => (DateTime?) x.Date > dataCopierViewModel.TransactionStartDate));
    if (dataCopierViewModel.TransactionEndDate.HasValue)
      query = query.Where<T>((Expression<Func<T, bool>>) (x => (DateTime?) x.Date > dataCopierViewModel.TransactionEndDate));
    await dataCopierViewModel.Copy<T>(context, query);
    if (typeof (T) != typeof (StockRevision))
    {
      query = (IQueryable<T>) null;
    }
    else
    {
      IEnumerable<string> revisionIds = await query.Select<T, string>((Expression<Func<T, string>>) (x => x.Id)).ExecuteAsync<string>();
      IQueryable<StockRevisionLine> query1 = context.Query<StockRevisionLine>().Where<StockRevisionLine>((Expression<Func<StockRevisionLine, bool>>) (x => x.DocType == typeof (StockRevisionLine).Name && x.Id == N1QlFunctions.Key(x) && revisionIds.Contains<string>(x.StockRevisionId)));
      await dataCopierViewModel.Copy<StockRevisionLine>(context, query1);
      query = (IQueryable<T>) null;
    }
  }

  public async Task Copy<T>(BucketContext context, IQueryable<T> query) where T : IModel
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
            await writer.CreateAsync(model);
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
