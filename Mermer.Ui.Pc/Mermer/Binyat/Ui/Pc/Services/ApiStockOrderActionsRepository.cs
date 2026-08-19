using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mermer.Data.Storage;
using Mermer.Http;
using Mermer.Warehousing.Ordering.Models;
using Mermer.Warehousing.Ordering.Services;

namespace Mermer.Ui.Pc.Services
{
    public class ApiStockOrderActionsRepository : IStockOrderActionsRepository
    {
        private readonly RestClient _restClient;
        private readonly IRepository<StockOrder> _ordersRepository;

        public ApiStockOrderActionsRepository(RestClient restClient, IRepository<StockOrder> ordersRepository)
        {
            _restClient = restClient;
            _ordersRepository = ordersRepository;
        }

        public async Task<IEnumerable<StockOrderAction>> GetAsync(string stockId)
        {
            if (string.IsNullOrEmpty(stockId)) return Enumerable.Empty<StockOrderAction>();

            try
            {
                // Загружаем открытые заказы и собираем количества по складам
                var orders = (await _ordersRepository.GetAsync(x => !x.IsCompleted && !x.IsDisabled)).ToList();
                var result = new List<StockOrderAction>();

                foreach (var order in orders)
                {
                    if (order.Lines == null) continue;
                    var matchingLines = order.Lines.Where(l => l.StockId == stockId);
                    foreach (var line in matchingLines)
                    {
                        result.Add(new StockOrderAction
                        {
                            WarehouseId = order.WarehouseId,
                            Quantity = line.Quantity
                        });
                    }
                }

                return result;
            }
            catch
            {
                return Enumerable.Empty<StockOrderAction>();
            }
        }
    }
}