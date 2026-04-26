// Decompiled with JetBrains decompiler
// Type: Payhas.Data.Synchronizer.Core.Services.IUsersRepositoryService
// Assembly: Payhas.Data.Synchronizer.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 51A01EA9-84E6-49FB-B6E1-8048825E2DB0
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Payhas.Data.Synchronizer.Core.dll

using Payhas.Data.Synchronizer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Payhas.Data.Synchronizer.Core.Services;

public interface IUsersRepositoryService
{
  Task<User> GetAsync(string id);

  Task<IEnumerable<User>> GetAsync();

  Task CreateAsync(User user);

  Task UpdateAsync(User user);
}
