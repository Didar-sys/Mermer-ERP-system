using Couchbase;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Mermer.Authorization.Models;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Data.Patcher;
using Mermer.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mermer.Core.Couch.Authentication;

public class AuthInitialDataCreator : IInitialDataCreator
{
    private readonly IPatcher _patcher;
    private readonly ICouchCluster _cluster;
    private readonly ICouchLocalChangesRepositoryService<CouchPatch> _localChangesRepositoryService;

    public AuthInitialDataCreator(
      IPatcher patcher,
      ICouchCluster cluster,
      ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService)
    {
        this._patcher = patcher;
        this._cluster = cluster;
        this._localChangesRepositoryService = localChangesRepositoryService;
    }

    public async Task CreateAsync()
    {
        using (IBucket bucket1 = this._cluster.OpenDefaultBucket())
        {
            // Проверяем, существует ли админ. Если индекс затупит – не страшно, нас спасет Upsert.
            if (!(await new BucketContext(bucket1).Query<User>().Where<User>(x => x.DocType == typeof(User).Name && x.IsAdmin).ExecuteAsync<User>()).Any<User>())
            {
                User user = new User();

                // Жесткий ID вместо случайного Guid ---
                user.Id = "User_admin";

                user.Username = "admin";
                user.Password = "admin".Hash();
                user.IsAdmin = true;
                User item = user;

                Patch patch = this._patcher.CreatePatch<User>(item, (User)null);
                if (patch == null)
                    return;

                ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = this._localChangesRepositoryService;
                CouchPatch[] patches = new CouchPatch[1];
                CouchPatch couchPatch = new CouchPatch();
                couchPatch.Id = patch.Id;
                couchPatch.Action = patch.Action;
                couchPatch.PropertyPatches = patch.PropertyPatches;
                couchPatch.SubListPatches = patch.SubListPatches;
                couchPatch.DocType = typeof(User).Name;
                couchPatch.Author = item.Username;
                patches[0] = couchPatch;
                IBucket bucket2 = bucket1;

                await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>)patches, bucket2);

                // Используем UpsertAsync вместо InsertAsync ---
                // Теперь, даже если кнопка сработает дважды, она не создаст клон, а просто обновит единого админа
                IDocumentResult<User> documentResult = await bucket1.UpsertAsync<User>((IDocument<User>)new Document<User>()
                {
                    Id = item.Id,
                    Content = item
                });
                // ------------------------------------------------------------------

                item = (User)null;
            }
        }
    }
}