// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Transactions.Services.TransactionCodeGenerationService
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.Transactions.Models;
using Payhas.Binyat.Transactions.Services;
using Payhas.Data.Tools.Barcode;
using Payhas.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Transactions.Services;

public class TransactionCodeGenerationService : ITransactionCodeGenerationService
{
  private readonly ICouchCluster _cluster;
  private readonly IConfigurator _configurator;

  public TransactionCodeGenerationService(ICouchCluster cluster, IConfigurator configurator)
  {
    this._cluster = cluster;
    this._configurator = configurator;
  }

  public async Task<string> GetNextCode()
  {
    string[] transactionTypes = new string[13]
    {
      "FundsSlip",
      "FundsTransfer",
      "DailyFundsRegistery",
      "ExpenseSlip",
      "StockSlip",
      "StockTransfer",
      "StockRevision",
      "StockOrder",
      "AggregatedStockOrder",
      "PartnerSlip",
      "PartnerTransfer",
      "Bill",
      "Invoice"
    };
    string code;
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      BucketContext context = new BucketContext(bucket);
      context.EndChangeTracking();
      AppSettings config = this._configurator.GetConfig<AppSettings>();
      int codeValue = config.LastTransactionCodeValue;
      IQueryable<TransactionModel> source;
      Expression<Func<IQueryable<TransactionModel>, int>> additionalExpression;
      do
      {
        ++codeValue;
        code = $"{config.LocalCodePrefix:D2}{codeValue:D10}";
        code += Symbology.CalculateChecksumDigit(code);
        string code1 = code;
        source = context.Query<TransactionModel>().Where<TransactionModel>((Expression<Func<TransactionModel, bool>>) (x => transactionTypes.Contains<string>(x.DocType) && x.Code == code1));
        additionalExpression = (Expression<Func<IQueryable<TransactionModel>, int>>) (x => x.Count<TransactionModel>());
      }
      while (await source.ExecuteAsync<TransactionModel, int>(additionalExpression) > 0);
      config.LastTransactionCodeValue = codeValue;
      this._configurator.SetConfig<AppSettings>(config);
      context = (BucketContext) null;
      config = (AppSettings) null;
    }
    string nextCode = code;
    code = (string) null;
    return nextCode;
  }
}
