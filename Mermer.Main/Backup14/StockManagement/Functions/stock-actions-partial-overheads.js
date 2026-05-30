
        var stockOverheadTotalsByStock = null;
        var allStockOverheadsTotal = 0;

        var stockLineTotalsByStock = null;
        var allStockLinesTotal = 0;

        if (doc.overheads && doc.overheads.length > 0) {
            stockOverheadTotalsByStock = doc.overheads
                .map(function(x) {
                    var currency = doc.currencyConvertions.find(function(el) {
                        return el.currencyId === x.currencyId;
                    });
                    return {
                        stockId: x.stockId,
                        total: x.amount * currency.multiplier / currency.divider
                    }
                })
                .reduce(function(list, x) {
                        var index = x.stockId ? x.stockId : '_general';
                        list[index] = (list[index] || 0) + x.total;
                        return list;
                    }, []);

            allStockOverheadsTotal = stockOverheadTotalsByStock['_general'];

            stockLineTotalsByStock = doc.lines
                .map(function (x) {
                    var currency = doc.currencyConvertions.find(function (el) {
                        return el.currencyId === x.currencyId;
                    });
                    return {
                        stockId: x.stockId,
                        total: x.quantity * x.price
                            * currency.multiplier
                            / currency.divider
                    }
                })
                .reduce(function (list, x) {
                    list['_all'] = (list['_all'] || 0) + x.total;
                    list[x.stockId] = (list[x.stockId] || 0) + x.total;
                    return list;
                }, []);

            allStockLinesTotal = stockLineTotalsByStock['_all'];
        }