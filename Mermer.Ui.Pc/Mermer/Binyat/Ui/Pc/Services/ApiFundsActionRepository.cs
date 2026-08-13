using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
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
            // 1. Читаем локальные данные из SQLite
            var localSlips = GetFromLocalDb();

            // 2. В фоне досылаем неотправленное из таблицы local_funds_slips и подтягиваем свежее с API
            _ = Task.Run(async () =>
            {
                try
                {
                    // --- 1. ДОСЫЛАЕМ НЕСИНХРОНИЗИРОВАННЫЕ ОРДЕРА ПРЯМО ИЗ local_funds_slips ---
                    var unsyncedSlips = GetUnsyncedFromLocalDb();
                    foreach (var slip in unsyncedSlips)
                    {
                        try
                        {
                            await _restClient.PostAsync("/api/finance/slips", slip);

                            // Отмечаем как синхронизированный в обоих местах
                            UpdateLocalSyncStatus(slip.Id, true);
                            LocalSqliteCache.SaveDocument("FundsSlip", slip.Id, slip, isSynced: true);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[FundsSlip Post Error]: {ex.Message}");
                        }
                    }

                    // --- 2. СКАЧИВАЕМ СВЕЖИЕ С СЕРВЕРА ---
                    var remoteSlips = await _restClient.GetAsync<List<FundsSlip>>("/api/finance/slips");
                    if (remoteSlips != null)
                    {
                        foreach (var slip in remoteSlips)
                        {
                            SaveToLocalDb(slip, isSynced: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[FundsSlip Sync Error]: {ex.Message}");
                }
            });

            return localSlips;
        }

        // Помощник для вытягивания несинхронизированных ордеров из local_funds_slips
        private List<FundsSlip> GetUnsyncedFromLocalDb()
        {
            var list = new List<FundsSlip>();
            try
            {
                using var connection = new SQLiteConnection(LocalSqliteCache.ConnectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT id, code, date, funds_slip_type, depository_id, user_name, description, lines_json, is_completed, is_disabled FROM local_funds_slips WHERE is_synced = 0;";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var slip = new FundsSlip
                    {
                        Id = reader.IsDBNull(0) ? null : reader.GetString(0),
                        Code = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Date = reader.IsDBNull(2) ? DateTime.Now : DateTime.Parse(reader.GetString(2)),
                        DepositoryId = reader.IsDBNull(4) ? null : reader.GetString(4),
                        UserName = reader.IsDBNull(5) ? null : reader.GetString(5),
                        Description = reader.IsDBNull(6) ? null : reader.GetString(6),
                        IsCompleted = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                        IsDisabled = !reader.IsDBNull(9) && reader.GetInt32(9) == 1
                    };

                    if (!reader.IsDBNull(7))
                    {
                        var json = reader.GetString(7);
                        var rawLines = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FundsSlipLine>>(json);
                        if (rawLines != null)
                        {
                            slip.Lines = new Mermer.Data.WatchedObservableCollection<FundsSlipLine>(rawLines);
                        }
                    }

                    list.Add(slip);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetUnsyncedFromLocalDb Error]: {ex.Message}");
            }
            return list;
        }

        private void UpdateLocalSyncStatus(string id, bool isSynced)
        {
            try
            {
                using var connection = new SQLiteConnection(LocalSqliteCache.ConnectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "UPDATE local_funds_slips SET is_synced = @synced WHERE id = @id;";
                command.Parameters.AddWithValue("@synced", isSynced ? 1 : 0);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            catch { }
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

            if (string.IsNullOrEmpty(entity.Id))
            {
                entity.Id = Guid.NewGuid().ToString();
            }

            // 1. Моментально сохраняем в локальный SQLite (со статусом is_synced = 0)
            SaveToLocalDb(entity, isSynced: false);

            // 2. Отправляем в PostgreSQL через API
            try
            {
                await _restClient.PostAsync("/api/finance/slips", entity);

                // Если API ответил успехом — помечаем в SQLite как синхронизировано
                SaveToLocalDb(entity, isSynced: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Save FundsSlip API Warning]: {ex.Message}. Saved to local SQLite.");
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

        #region SQLite Helpers
        private void SaveToLocalDb(FundsSlip slip, bool isSynced)
        {
            try
            {
                using (var connection = new SQLiteConnection(LocalSqliteCache.ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO local_funds_slips (
                                id, code, date, funds_slip_type, depository_id, user_name, description, lines_json, is_completed, is_disabled, is_synced, updated_at
                            ) VALUES (
                                @id, @code, @date, @type, @depositoryId, @userName, @description, @linesJson, @isCompleted, @isDisabled, @isSynced, @updatedAt
                            )
                            ON CONFLICT(id) DO UPDATE SET
                                code = @code,
                                date = @date,
                                funds_slip_type = @type,
                                depository_id = @depositoryId,
                                user_name = @userName,
                                description = @description,
                                lines_json = @linesJson,
                                is_completed = @isCompleted,
                                is_disabled = @isDisabled,
                                is_synced = @isSynced,
                                updated_at = @updatedAt;
                        ";

                        command.Parameters.AddWithValue("@id", slip.Id ?? Guid.NewGuid().ToString());
                        command.Parameters.AddWithValue("@code", slip.Code ?? "");
                        command.Parameters.AddWithValue("@date", slip.Date.ToString("o"));
                        command.Parameters.AddWithValue("@type", slip.SlipType.ToString());
                        command.Parameters.AddWithValue("@depositoryId", slip.DepositoryId ?? "");
                        command.Parameters.AddWithValue("@userName", slip.UserName ?? "admin");
                        command.Parameters.AddWithValue("@description", slip.Description ?? "");
                        command.Parameters.AddWithValue("@linesJson", JsonSerializer.Serialize(slip.Lines ?? new Mermer.Data.WatchedObservableCollection<FundsSlipLine>()));
                        command.Parameters.AddWithValue("@isCompleted", slip.IsCompleted ? 1 : 0);
                        command.Parameters.AddWithValue("@isDisabled", slip.IsDisabled ? 1 : 0);
                        command.Parameters.AddWithValue("@isSynced", isSynced ? 1 : 0);
                        command.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("o"));

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLite Save Error]: {ex.Message}");
            }
        }

        private List<FundsSlip> GetFromLocalDb()
        {
            var result = new List<FundsSlip>();
            try
            {
                using (var connection = new SQLiteConnection(LocalSqliteCache.ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT id, code, date, funds_slip_type, depository_id, user_name, description, lines_json, is_completed, is_disabled FROM local_funds_slips;";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var slip = new FundsSlip
                                {
                                    Id = reader.GetString(0),
                                    Code = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                    Date = reader.IsDBNull(2) ? DateTime.Now : DateTime.Parse(reader.GetString(2)),
                                    DepositoryId = reader.IsDBNull(4) ? null : reader.GetString(4),
                                    UserName = reader.IsDBNull(5) ? "admin" : reader.GetString(5),
                                    Description = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                    IsCompleted = !reader.IsDBNull(8) && reader.GetInt32(8) == 1,
                                    IsDisabled = !reader.IsDBNull(9) && reader.GetInt32(9) == 1
                                };

                                string linesJson = reader.IsDBNull(7) ? null : reader.GetString(7);
                                if (!string.IsNullOrEmpty(linesJson))
                                {
                                    try
                                    {
                                        var lines = JsonSerializer.Deserialize<List<FundsSlipLine>>(linesJson);
                                        if (lines != null)
                                        {
                                            slip.Lines = new Mermer.Data.WatchedObservableCollection<FundsSlipLine>(lines);
                                        }
                                    }
                                    catch { }
                                }

                                // Восстанавливаем конвертацию валют для расчета Total
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
                                    slip.DisplayCurrencyId = slip.Lines.FirstOrDefault()?.CurrencyId;
                                }

                                result.Add(slip);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SQLite Read Error]: {ex.Message}");
            }

            return result;
        }
        #endregion
    }
}