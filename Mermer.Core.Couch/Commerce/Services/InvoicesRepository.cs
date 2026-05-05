// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Commerce.Services.InvoicesRepository
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Commerce.Models;
using Mermer.Commerce.Services;
using Mermer.Common.Services;
using Mermer.Common.Settings;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.CRM.Models;
using Mermer.CRM.Services;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using Mermer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Commerce.Services;

public class InvoicesRepository : 
  CouchRepositoryWithFacet<Invoice>,
  IInvoicesRepository,
  IRepository<Invoice>,
  IReadOnlyRepository<Invoice>
{
  private readonly IPartnerActionsRepository _partnerActionsRepository;
  private readonly InvoicesInfoView _infoView;
  private readonly InvoicesPaymentsView _paymentsView;
  private readonly IConfigurator _configurator;
  private readonly IAuthorizationService _authService;

  public InvoicesRepository(
    IPatcher patcher,
    ICouchCluster cluster,
    IConfigurator configurator,
    ILoginService loginService,
    IAuthorizationService authService,
    IValidator<Invoice> validator,
    IListAuthorizer<Invoice> authorizer,
    IPartnerActionsRepository partnerActionsRepository,
    IDocumentChangeListener changeListener,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService)
    : base(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
  {
    this._partnerActionsRepository = partnerActionsRepository;
    this._infoView = new InvoicesInfoView(cluster, loginService, authService);
    this._paymentsView = new InvoicesPaymentsView(cluster, loginService, authService);
    this._configurator = configurator;
    this._authService = authService;
  }

  public override async Task ValidateAsync(Invoice model)
  {
    InvoicesRepository invoicesRepository = this;
    AppSettings configAsync = await invoicesRepository._configurator.GetConfigAsync<AppSettings>();
    // ISSUE: reference to a compiler-generated method
    await invoicesRepository.Validator.AssertValidAsync<Invoice>(model, new Action<ValidationContext<Invoice>>(invoicesRepository.\u003CValidateAsync\u003Eb__6_0));
  }

  public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
  {
    return this.GetFacetsFromView("transaction", "facets", fields);
  }

  public Task<int> CountInfoAsync(DateTime from, DateTime till)
  {
    this.Authorizer.Authorize();
    return this._infoView.CountAsync(from, till);
  }

  public Task<IEnumerable<InvoiceInfo>> GetInfoAsync(DateTime from, DateTime till)
  {
    this.Authorizer.Authorize();
    return this._infoView.GetAsync(from, till);
  }

  public Task<int> CountPaymentInfoAsync(
    DateTime from,
    DateTime till,
    string officeId,
    string partnerId)
  {
    this.Authorizer.Authorize();
    return this._paymentsView.CountAsync(from, till, officeId, partnerId);
  }

  public async Task<IEnumerable<InvoicePaymentInfo>> GetPaymentInfoAsync(
    DateTime from,
    DateTime till,
    string officeId,
    string partnerId)
  {
    InvoicesRepository invoicesRepository = this;
    invoicesRepository.Authorizer.Authorize();
    List<InvoicePaymentInfo> invoicePayments = (await invoicesRepository._paymentsView.GetAsync(from, till, officeId, partnerId)).ToList<InvoicePaymentInfo>();
    string[] array = invoicePayments.Select<InvoicePaymentInfo, string>((Func<InvoicePaymentInfo, string>) (x => x.PartnerId)).Where<string>((Func<string, bool>) (x => !string.IsNullOrEmpty(x))).Distinct<string>().ToArray<string>();
    Dictionary<string, PartnerActionInfo[]> byPartnersAsync = await invoicesRepository._partnerActionsRepository.GetByPartnersAsync(officeId, array);
    foreach (InvoicePaymentInfo invoicePaymentInfo in invoicePayments)
      invoicePaymentInfo.UpdatePaymentInfo(byPartnersAsync[invoicePaymentInfo.PartnerId]);
    IEnumerable<InvoicePaymentInfo> paymentInfoAsync = (IEnumerable<InvoicePaymentInfo>) invoicePayments;
    invoicePayments = (List<InvoicePaymentInfo>) null;
    return paymentInfoAsync;
  }
}
