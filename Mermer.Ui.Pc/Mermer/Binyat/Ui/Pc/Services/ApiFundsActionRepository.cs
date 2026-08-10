using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Finance.Models;
using Mermer.Http;
using Mermer.Transactions.Models;

namespace Mermer.Ui.Pc.Services
{
    public class ApiFundsActionRepository :
        IRepository<FundsSlip>,
        IReadOnlyRepository<FundsSlip>,
        IRepositoryWithFacets<FundsSlip>
    {
        private readonly RestClient _restClient;

        public ApiFundsActionRepository(RestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        public async Task<IEnumerable<FundsSlip>> GetAsync() => await GetAllAsync();

        public async Task<IEnumerable<FundsSlip>> GetAllAsync()
        {
            try
            {
                var result = await _restClient.GetAsync<List<FundsSlip>>("/api/finance/slips");
                if (result != null)
                {
                    foreach (var slip in result)
                    {
                        // 1. Инициализируем курсы 1:1, чтобы Mermer правильно пересчитал Total
                        if (slip.CurrencyConvertions == null)
                        {
                            slip.CurrencyConvertions = new Mermer.Data.WatchedObservableCollection<CurrencyConvertion>();
                        }

                        if (slip.Lines != null && slip.Lines.Any())
                        {
                            foreach (var line in slip.Lines)
                            {
                                if (!string.IsNullOrEmpty(line.CurrencyId) &&
                                    !slip.CurrencyConvertions.Any(c => c.CurrencyId == line.CurrencyId))
                                {
                                    slip.CurrencyConvertions.Add(new CurrencyConvertion
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        CurrencyId = line.CurrencyId,
                                        Multiplier = 1,
                                        Divider = 1
                                    });
                                }
                            }
                        }

                        // 2. Указываем отображаемую валюту для ордера (если не задана)
                        if (string.IsNullOrEmpty(slip.DisplayCurrencyId) && slip.Lines != null && slip.Lines.Any())
                        {
                            slip.DisplayCurrencyId = slip.Lines.FirstOrDefault()?.CurrencyId;
                        }
                    }
                }
                return result ?? Enumerable.Empty<FundsSlip>();
            }
            catch
            {
                return Enumerable.Empty<FundsSlip>();
            }
        }

        public async Task<FundsSlip> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var all = await GetAllAsync();
            return all.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<FundsSlip>> GetAsync(string[] ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<FundsSlip>();
            var all = await GetAllAsync();
            var set = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
            return all.Where(d => set.Contains(d.Id));
        }

        public async Task<IEnumerable<FundsSlip>> GetAsync(params Expression<Func<FundsSlip, bool>>[] predicates)
        {
            var all = await GetAllAsync();
            var query = all.AsQueryable();
            if (predicates != null)
            {
                foreach (var p in predicates) if (p != null) query = query.Where(p);
            }
            return query.ToList();
        }

        public async Task<int> CountAsync(params Expression<Func<FundsSlip, bool>>[] predicates)
        {
            var res = await GetAsync(predicates);
            return res.Count();
        }

        public async Task SaveAsync(FundsSlip entity)
        {
            if (entity == null) return;
            try
            {
                if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();
                await _restClient.PostAsync("/api/finance/slips", entity);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Save FundsSlip Error]: {ex.Message}");
                throw;
            }
        }

        public async Task CreateAsync(FundsSlip entity) => await SaveAsync(entity);
        public async Task UpdateAsync(FundsSlip entity) => await SaveAsync(entity);
        public Task DeleteAsync(string id) => Task.CompletedTask;

        public async Task<Dictionary<string, Dictionary<string, int>>> GetFacets(params string[] facetFields)
        {
            var allSlips = await GetAllAsync();
            var result = new Dictionary<string, Dictionary<string, int>>();

            foreach (var field in facetFields)
            {
                if (field == "Date")
                {
                    var dateFacet = new Dictionary<string, int>();
                    var now = DateTime.Now;

                    dateFacet["#Today"] = allSlips.Count(x => x.Date.Date == now.Date);
                    dateFacet["#This Week"] = allSlips.Count(x => x.Date >= now.AddDays(-7));
                    dateFacet["#This Month"] = allSlips.Count(x => x.Date.Month == now.Month && x.Date.Year == now.Year);
                    dateFacet["#This Year"] = allSlips.Count(x => x.Date.Year == now.Year);
                    dateFacet["#All Records"] = allSlips.Count();

                    result["Date"] = dateFacet;
                }
                else
                {
                    result[field] = new Dictionary<string, int>();
                }
            }

            return result;
        }
    }
}