using Mermer.FundsManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable disable
namespace Mermer.FundsManagement.Services;

public interface IFundsActionsRepository
{
    Task<int> CountAsync(
      DateTime? startDate,
      DateTime? endDate,
      string currencyId,
      params string[] depositoryIds);

    Task<IEnumerable<FundsAction>> GetAsync(
      DateTime? startDate,
      DateTime? endDate,
      string currencyId,
      params string[] depositoryIds);
}