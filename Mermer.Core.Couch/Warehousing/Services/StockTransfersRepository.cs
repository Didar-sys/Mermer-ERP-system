using FluentValidation;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Common.Settings;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Warehousing.Models;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using Mermer.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.Warehousing.Services;

public class StockTransfersRepository : CouchRepositoryWithFacet<StockTransfer>
{
    private readonly IConfigurator _configurator;
    private readonly IAuthorizationService _authService;

    public StockTransfersRepository(
        ICouchCluster cluster,
        IValidator<StockTransfer> validator,
        IConfigurator configurator,
        IAuthorizationService authService,
        IListAuthorizer<StockTransfer> authorizer,
        IDocumentChangeListener changeListener,
        ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService,
        IPatcher patcher,
        ILoginService loginService)
        : base(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
    {
        _configurator = configurator;
        _authService = authService;
    }

    public override async Task ValidateAsync(StockTransfer model)
    {
        var config = await _configurator.GetConfigAsync<AppSettings>();
        await Validator.ValidateAndThrowAsync(model); // Используем стандартный валидтор
    }

    public override Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        return GetFacetsFromView("transaction", "facets", fields);
    }
}