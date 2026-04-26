// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.StockManagement.Services.StockCodeGenerationService
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Payhas.Binyat.Common.Settings;
using Payhas.Binyat.Core.Couch.Common;
using Payhas.Binyat.StockManagement.Models;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Data.Tools.Barcode;
using Payhas.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.StockManagement.Services;

public class StockCodeGenerationService : IStockCodeGenerationService
{
  private readonly ICouchCluster _cluster;
  private readonly IConfigurator _configurator;

  public StockCodeGenerationService(ICouchCluster cluster, IConfigurator configurator)
  {
    this._cluster = cluster;
    this._configurator = configurator;
  }

  public async Task<string> GetNextCode()
  {
    string code;
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      BucketContext context = new BucketContext(bucket);
      context.EndChangeTracking();
      AppSettings config = this._configurator.GetConfig<AppSettings>();
      int codeValue = config.LastStockCodeValue;
      IQueryable<Stock> source;
      Expression<Func<IQueryable<Stock>, int>> additionalExpression;
      do
      {
        ++codeValue;
        code = $"{config.LocalCodePrefix:D2}{codeValue:D5}";
        code += Symbology.CalculateChecksumDigit(code);
        string code1 = code;
        source = context.Query<Stock>().Where<Stock>((Expression<Func<Stock, bool>>) (x => x.DocType == "Stock" && x.Code == code1));
        additionalExpression = (Expression<Func<IQueryable<Stock>, int>>) (x => x.Count<Stock>());
      }
      while (await source.ExecuteAsync<Stock, int>(additionalExpression) > 0);
      config.LastStockCodeValue = codeValue;
      this._configurator.SetConfig<AppSettings>(config);
      context = (BucketContext) null;
      config = (AppSettings) null;
    }
    string nextCode = code;
    code = (string) null;
    return nextCode;
  }
}
