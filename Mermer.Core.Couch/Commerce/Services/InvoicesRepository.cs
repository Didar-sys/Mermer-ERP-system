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
        _partnerActionsRepository = partnerActionsRepository;
        _infoView = new InvoicesInfoView(cluster, loginService, authService);
        _paymentsView = new InvoicesPaymentsView(cluster, loginService, authService);
        _configurator = configurator;
        _authService = authService;
    }

    public override async Task ValidateAsync(Invoice model)
    {
        AppSettings config = await _configurator.GetConfigAsync<AppSettings>();

        await Task.CompletedTask;
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

    public Task<int> CountPaymentInfoAsync(DateTime from, DateTime till, string officeId, string partnerId)
    {
        this.Authorizer.Authorize();
        return this._paymentsView.CountAsync(from, till, officeId, partnerId);
    }

    public async Task<IEnumerable<InvoicePaymentInfo>> GetPaymentInfoAsync(DateTime from, DateTime till, string officeId, string partnerId)
    {
        this.Authorizer.Authorize();

        // Очищенная и упрощенная работа с коллекциями
        var invoicePayments = (await this._paymentsView.GetAsync(from, till, officeId, partnerId)).ToList();

        string[] array = invoicePayments
            .Select(x => x.PartnerId)
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct()
            .ToArray();

        var byPartnersAsync = await this._partnerActionsRepository.GetByPartnersAsync(officeId, array);

        foreach (var invoicePaymentInfo in invoicePayments)
        {
            invoicePaymentInfo.UpdatePaymentInfo(byPartnersAsync[invoicePaymentInfo.PartnerId]);
        }

        return invoicePayments;
    }
}