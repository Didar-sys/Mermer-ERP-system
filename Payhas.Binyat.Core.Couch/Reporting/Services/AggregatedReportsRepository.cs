// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Reporting.Services.AggregatedReportsRepository
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Payhas.Binyat.CRM.Services;
using Payhas.Binyat.Enterprise.Models;
using Payhas.Binyat.FundsManagement.Services;
using Payhas.Binyat.Reporting.Models;
using Payhas.Binyat.Reporting.Models.Authorizers;
using Payhas.Binyat.Reporting.Services;
using Payhas.Binyat.StockManagement.Services;
using Payhas.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Reporting.Services;

public class AggregatedReportsRepository : IAggregatedReportsRepository
{
  private readonly AggregatedReportAuthorizer _authorizer;
  private readonly IRepository<Warehouse> _warehousesRepository;
  private readonly IRepository<Depository> _depositoriesRepository;
  private readonly IStockBalancesAggregatedRepository _stockBalancesRepository;
  private readonly IFundsBalancesRepository _fundsBalancesRepository;
  private readonly IPartnerBalancesRepository _partnerBalancesRepository;

  public AggregatedReportsRepository(
    AggregatedReportAuthorizer authorizer,
    IRepository<Warehouse> warehousesRepository,
    IRepository<Depository> depositoriesRepository,
    IStockBalancesAggregatedRepository stockBalancesRepository,
    IFundsBalancesRepository fundsBalancesRepository,
    IPartnerBalancesRepository partnerBalancesRepository)
  {
    this._authorizer = authorizer;
    this._warehousesRepository = warehousesRepository;
    this._depositoriesRepository = depositoriesRepository;
    this._stockBalancesRepository = stockBalancesRepository;
    this._fundsBalancesRepository = fundsBalancesRepository;
    this._partnerBalancesRepository = partnerBalancesRepository;
  }

  public async Task<AggregatedReport> GetAsync(
    string[] officeIds,
    DateTime dateFrom,
    DateTime dateTill)
  {
    this._authorizer.Authorize((string) null);
    AggregatedReport report = new AggregatedReport();
    if (officeIds == null || !((IEnumerable<string>) officeIds).Any<string>())
      return report;
    string[] depositoryIds = (await this._depositoriesRepository.GetAsync((Expression<Func<Depository, bool>>) (x => officeIds.Contains<string>(x.OfficeId)))).Select<Depository, string>((Func<Depository, string>) (x => x.Id)).ToArray<string>();
    string[] warehouseIds = (await this._warehousesRepository.GetAsync((Expression<Func<Warehouse, bool>>) (x => officeIds.Contains<string>(x.OfficeId)))).Select<Warehouse, string>((Func<Warehouse, string>) (x => x.Id)).ToArray<string>();
    AggregatedReport aggregatedReport = report;
    aggregatedReport.FundsReport = await this._fundsBalancesRepository.GetByTypeAggregatedAsync(depositoryIds, new DateTime?(dateFrom), new DateTime?(dateTill));
    aggregatedReport = (AggregatedReport) null;
    aggregatedReport = report;
    aggregatedReport.StocksReport = await this._stockBalancesRepository.GetByTypeAggregatedAsync(warehouseIds, dateFrom, dateTill);
    aggregatedReport = (AggregatedReport) null;
    aggregatedReport = report;
    aggregatedReport.PartnersReport = await this._partnerBalancesRepository.GetByTypeAggregatedAsync(officeIds, dateFrom, dateTill);
    aggregatedReport = (AggregatedReport) null;
    return report;
  }
}
