using Couchbase;
using Couchbase.Core;
using FluentValidation;
using Mermer.Authorization.Models;
using Mermer.Authorization.Services;
using Mermer.Common.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Data.Authorizers;
using Mermer.Data.Patcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.Authentication.Services;

public class UsersRepository(
    IPatcher patcher,
    ICouchCluster cluster,
    IValidator<User> validator,
    ILoginService loginService,
    IListAuthorizer<User> authorizer,
    IDocumentChangeListener changeListener,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService) :
    CouchRepository<User>(patcher, cluster, validator, loginService, authorizer, changeListener, localChangesRepositoryService)
{
    public override Task CreateAsync(User model)
    {
        model.Password = model.Password.Hash();
        return base.CreateAsync(model);
    }

    public override async Task UpdateAsync(User model)
    {
        if (string.IsNullOrEmpty(model.Password))
        {
            using (IBucket bucket = this.Cluster.OpenDefaultBucket())
            {
                IDocumentResult<User> documentAsync = await bucket.GetDocumentAsync<User>(model.Id);
                if (documentAsync?.Content != null)
                    model.Password = documentAsync.Content.Password;
            }
        }
        await base.UpdateAsync(model);
        model.Password = null;
    }

    public override async Task<User> GetAsync(string id)
    {
        User result = await base.GetAsync(id);
        if (result != null)
            result.Password = null;
        return result;
    }

    public override async Task<IEnumerable<User>> GetAsync(params Expression<Func<User, bool>>[] predicates)
    {
        IEnumerable<User> items = await base.GetAsync(predicates);
        var userList = items.ToList();

        foreach (User user in userList)
        {
            user.Password = null;
        }

        return userList;
    }
}