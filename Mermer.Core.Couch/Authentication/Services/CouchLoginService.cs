// Decompiled with JetBrains decompiler
// Type: Mermer.Core.Couch.Authentication.Services.CouchLoginService
// Assembly: Mermer.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.Core.Couch.dll

using Couchbase;
using Couchbase.Core;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;
using Mermer.Authorization.Models;
using Mermer.Common.Services;
using Mermer.Core.Authorization.Services;
using Mermer.Core.Couch.Changes;
using Mermer.Core.Couch.Changes.Services;
using Mermer.Core.Couch.Common;
using Mermer.Data.Patcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Core.Couch.Authentication.Services;

public class CouchLoginService : LoginService
{
  private readonly IPatcher _patcher;
  private readonly ICouchCluster _cluster;
  private readonly ILocalizationService _localizationService;
  private readonly ICouchLocalChangesRepositoryService<CouchPatch> _localChangesRepositoryService;

  public CouchLoginService(
    IPatcher patcher,
    ICouchCluster cluster,
    ILocalizationService localizationService,
    ICouchLocalChangesRepositoryService<CouchPatch> localChangesRepositoryService)
  {
    this._patcher = patcher;
    this._cluster = cluster;
    this._localizationService = localizationService;
    this._localChangesRepositoryService = localChangesRepositoryService;
  }

  public override async Task UpdatePassword(string currentPassword, string newPassword)
  {
    CouchLoginService couchLoginService = this;
    using (IBucket bucket1 = couchLoginService._cluster.OpenDefaultBucket())
    {
      // ISSUE: explicit non-virtual call
      IDocumentResult<User> user = await bucket1.GetDocumentAsync<User>(__nonvirtual (couchLoginService.Session).UserId);
      if (user.Content.Password != currentPassword.Hash())
        throw new Exception(couchLoginService._localizationService.GetText("Wrong password!"));
      user.Content.Password = newPassword.Hash();
      ICouchLocalChangesRepositoryService<CouchPatch> repositoryService = couchLoginService._localChangesRepositoryService;
      CouchPatch[] patches = new CouchPatch[1];
      CouchPatch couchPatch = new CouchPatch();
      // ISSUE: explicit non-virtual call
      couchPatch.Id = __nonvirtual (couchLoginService.Session).UserId;
      couchPatch.Action = PatchAction.Update;
      couchPatch.PropertyPatches = new Dictionary<string, object>()
      {
        {
          "Password",
          (object) user.Content.Password
        }
      };
      couchPatch.DocType = typeof (User).Name;
      // ISSUE: explicit non-virtual call
      couchPatch.Author = __nonvirtual (couchLoginService.Session).Username;
      patches[0] = couchPatch;
      IBucket bucket2 = bucket1;
      await repositoryService.StorePatchesAsync((IEnumerable<CouchPatch>) patches, bucket2);
      IDocumentResult<User> documentResult = await bucket1.ReplaceAsync<User>((IDocument<User>) user.Document, ReplicateTo.One, PersistTo.One);
      user = (IDocumentResult<User>) null;
    }
  }

  protected override async Task<User> GetUser(string username, string password)
  {
    User user;
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      string passwordHash = password.Hash();
      user = await new BucketContext(bucket).Query<User>().Where<User>((Expression<Func<User, bool>>) (x => x.DocType == "User" && x.Id == N1QlFunctions.Key(x) && x.Username == username && x.Password == passwordHash)).ExecuteAsync<User, User>((Expression<Func<IQueryable<User>, User>>) (q => q.Single<User>()));
    }
    return user;
  }

  protected override async Task<IEnumerable<Role>> GetRoles(IEnumerable<string> roles)
  {
    IEnumerable<Role> roles1;
    using (IBucket bucket = this._cluster.OpenDefaultBucket())
    {
      List<Role> list = (await new BucketContext(bucket).Query<Role>().UseKeys<Role>(roles).ExecuteAsync<Role>()).ToList<Role>();
      foreach (Role role in list)
        role.Authorizations = role.Authorizations.ToDictionary<KeyValuePair<string, int>, string, int>((Func<KeyValuePair<string, int>, string>) (x => x.Key.First<char>().ToString().ToUpper() + x.Key.Substring(1)), (Func<KeyValuePair<string, int>, int>) (x => x.Value));
      roles1 = (IEnumerable<Role>) list;
    }
    return roles1;
  }
}
