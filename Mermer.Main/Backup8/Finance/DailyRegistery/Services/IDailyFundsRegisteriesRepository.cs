// Decompiled with JetBrains decompiler
// Type: Mermer.Finance.DailyRegistery.Services.IDailyFundsRegisteriesRepository
// Assembly: Mermer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 19F85A6C-D40F-439D-9478-41F01000D67D
// Assembly location: C:\Users\Admin\AppData\Local\Temp\Bofyhol\f9d7aa10a6\lib\net45\Mermer.dll

using Mermer.Finance.DailyRegistery.Models;
using Mermer.Data.Storage;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.Finance.DailyRegistery.Services;

public interface IDailyFundsRegisteriesRepository : 
  IRepository<DailyFundsRegistery>,
  IReadOnlyRepository<DailyFundsRegistery>
{
  Task<IEnumerable<DailyFundsRegisteryInfo>> GetAsync(
    params Expression<Func<DailyFundsRegistery, bool>>[] predicates);
}
