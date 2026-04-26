// Decompiled with JetBrains decompiler
// Type: Payhas.Binyat.Core.Couch.Changes.Services.ICouchLocalChangesRepositoryService`1
// Assembly: Payhas.Binyat.Core.Couch, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1349257C-46CD-4839-9154-FBCC3222CF25
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Binyat.Core.Couch.dll

using Couchbase.Core;
using Payhas.Data.Synchronizer.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Binyat.Core.Couch.Changes.Services;

public interface ICouchLocalChangesRepositoryService<T> : 
  ILocalChangesRepositoryService<T>,
  IChangesRepositoryService<T>
  where T : class
{
  Task StorePatchesAsync(IEnumerable<T> patches, IBucket bucket);
}
