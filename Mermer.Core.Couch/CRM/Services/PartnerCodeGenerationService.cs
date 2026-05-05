// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.CRM.Services.PartnerCodeGenerationService
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Mermer.Common.Settings;
using Mermer.Core.Couch.Common;
using Mermer.CRM.Models;
using Mermer.CRM.Services;
using Mermer.Data.Tools.Barcode;
using Mermer.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.CRM.Services;

public class PartnerCodeGenerationService : IPartnerCodeGenerationService
{
  private readonly ICouchCluster _cluster;
  private readonly IConfigurator _configurator;

  public PartnerCodeGenerationService(ICouchCluster cluster, IConfigurator configurator)
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
      int codeValue = config.LastPartnerCodeValue;
      IQueryable<Partner> source;
      Expression<Func<IQueryable<Partner>, int>> additionalExpression;
      do
      {
        ++codeValue;
        code = $"{config.LocalCodePrefix:D2}{codeValue:D5}";
        code += Symbology.CalculateChecksumDigit(code);
        string code1 = code;
        source = context.Query<Partner>().Where<Partner>((Expression<Func<Partner, bool>>) (x => x.DocType == "Partner" && x.Code == code1));
        additionalExpression = (Expression<Func<IQueryable<Partner>, int>>) (x => x.Count<Partner>());
      }
      while (await source.ExecuteAsync<Partner, int>(additionalExpression) > 0);
      config.LastPartnerCodeValue = codeValue;
      this._configurator.SetConfig<AppSettings>(config);
      context = (BucketContext) null;
      config = (AppSettings) null;
    }
    string nextCode = code;
    code = (string) null;
    return nextCode;
  }
}
