using Mermer.Commerce.Models;
using Mermer.Data.Storage;
using Mermer.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mermer.Ui.Pc.Services;

public class ApiBillsRepository : IRepositoryWithFacets<Bill>, IRepository<Bill>, IReadOnlyRepository<Bill>
{
    private readonly RestClient _restClient;

    public ApiBillsRepository(RestClient restClient)
    {
        _restClient = restClient;
    }

    // --- IReadOnlyRepository<Bill> ---

    public async Task<Bill> GetAsync(string id)
    {
        return await _restClient.GetAsync<Bill>($"/api/bills/{id}");
    }

    public async Task<IEnumerable<Bill>> GetAsync(string[] ids)
    {
        if (ids == null || !ids.Any()) return Enumerable.Empty<Bill>();
        var all = await _restClient.GetAsync<IEnumerable<Bill>>("/api/bills") ?? Enumerable.Empty<Bill>();
        var idSet = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        return all.Where(b => idSet.Contains(b.Id));
    }

    public async Task<IEnumerable<Bill>> GetAsync(params Expression<Func<Bill, bool>>[] predicates)
    {
        var bills = await _restClient.GetAsync<IEnumerable<Bill>>("/api/bills") ?? Enumerable.Empty<Bill>();

        if (predicates != null && predicates.Any())
        {
            foreach (var predicate in predicates)
            {
                if (predicate != null)
                {
                    var func = predicate.Compile();
                    bills = bills.Where(func);
                }
            }
            bills = bills.ToList();
        }

        return bills;
    }

    // ИСПРАВЛЕНО: Теперь счетчики плиток считают реальное количество записей по предикатам!
    public async Task<int> CountAsync(params Expression<Func<Bill, bool>>[] predicates)
    {
        var result = await GetAsync(predicates);
        return result.Count();
    }

    // --- IRepository<Bill> ---

    public async Task CreateAsync(Bill model)
    {
        await _restClient.PostAsync<Bill>("/api/bills", model);
    }

    public async Task UpdateAsync(Bill model)
    {
        await _restClient.PutAsync<Bill>($"/api/bills/{model.Id}", model);
    }

    public async Task<Bill> SaveAsync(Bill entity)
    {
        if (string.IsNullOrEmpty(entity.Id))
        {
            return await _restClient.PostAsync<Bill>("/api/bills", entity);
        }
        return await _restClient.PutAsync<Bill>($"/api/bills/{entity.Id}", entity);
    }

    public async Task DeleteAsync(string id)
    {
        await _restClient.DeleteAsync($"/api/bills/{id}");
    }

    // --- IRepositoryWithFacets<Bill> ---

    public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] fields)
    {
        var dict = new Dictionary<string, Dictionary<string, int>>();
        if (fields != null)
        {
            foreach (var f in fields) dict[f] = new Dictionary<string, int>();
        }
        return await Task.FromResult(dict);
    }
}