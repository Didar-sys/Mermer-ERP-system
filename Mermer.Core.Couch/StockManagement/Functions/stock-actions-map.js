function (doc, meta) {
    if (doc.id === meta.id && doc.docType && [
        'Invoice',
        'StockSlip',
        'StockTransfer'].indexOf(doc.docType) > -1 &&
        doc.lines) {
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

        for (var i = 0; i < doc.lines.length; ++i) {
            const line = doc.lines[i];

            const actionUnit = doc.stockUnitConvertions.find(function (el) {
                return el.stockId === line.stockId && el.unitId === line.unitId;
            });
            const actionQuantity = line.quantity
                * actionUnit.multiplier
                / actionUnit.divider;

            const actionCurrency = doc.currencyConvertions.find(function (el) {
                return el.currencyId === line.currencyId;
            });
            const actionPrice = line.price
                * actionCurrency.multiplier
                / actionCurrency.divider
                / actionUnit.multiplier
                * actionUnit.divider;

            const sameStockOverheadsTotal = stockOverheadTotalsByStock
                ? stockOverheadTotalsByStock[line.stockId] || 0
                : 0;
            const sameStockLinesTotal = stockLineTotalsByStock
                ? stockLineTotalsByStock[line.stockId] || 0
                : 0;

            const lineTotal = actionQuantity * actionPrice;

            const allstockLinesRate = allStockLinesTotal === 0 ? 0 : +(lineTotal / allStockLinesTotal).toFixed(2);
            const sameStockLinesRate = sameStockLinesTotal === 0 ? 0 : +(lineTotal / sameStockLinesTotal).toFixed(2);
            const actionOverhead = +((allstockLinesRate * allStockOverheadsTotal) +
                (sameStockLinesRate * sameStockOverheadsTotal)).toFixed(2);

            const key = {
                date: doc.date,
                type: doc.type,
                userId: doc.userId,
                warehouseId: doc.warehouseId,
                stockId: line.stockId
            }
            const value = {
                tId: doc.id,
                tCode: doc.code,
                tUserName: doc.userName,

                tIsCompleted: doc.isCompleted,
                tIsDisabled: doc.isDisabled,

                tGroup: doc.group,
                tTags: doc.tags,

                aId: line.id,
                aSourceId: line.sourceId,

                aPrice: actionPrice,

                aIncome: doc.isStockIncome
                    ? actionQuantity
                    : 0,

                aExpense: !doc.isStockIncome
                    ? actionQuantity
                    : 0,

                aDiscount: 0,

                aOverhead: actionOverhead
            };

            if (doc.docType === 'StockTransfer') {
                key.type = 'StockTransferSource';
                value.aRWId = doc.destinationWarehouseId;

                emit([key.type, key.userId, key.warehouseId, key.stockId, key.date], value);

                key.type = 'StockTransferDestination';
                key.warehouseId = doc.destinationWarehouseId;

                value.aRWId = doc.warehouseId;
                value.aId = line.receivedId;
                value.aSourceId = line.id;
                value.aDiscount = 0;
                value.aOverhead = 0;

                var receivedActionUnit = doc.stockUnitConvertions.find(function (el) {
                    return el.stockId === line.stockId
                        && el.unitId === line.receivedUnitId;
                });
                var receivedActionQuantity = line.receivedQuantity
                    * receivedActionUnit.multiplier
                    / receivedActionUnit.divider;

                var receivedActionPrice = line.price
                    * actionCurrency.multiplier
                    / actionCurrency.divider

                    / receivedActionUnit.multiplier
                    * receivedActionUnit.divider;

                value.aIncome = receivedActionQuantity;
                value.aExpense = 0;
                value.aPrice = receivedActionPrice;

            } else {
                if (doc.docType === 'Invoice') {
                    value.tIsCash = doc.isCash;
                    value.aRPId = doc.partnerId;

                    var discounRate = doc.actionTotal === 0 ? 0
                        : +(doc.actionGrandTotal / doc.actionTotal).toFixed(2);

                    value.aPrice = +(actionPrice * discounRate).toFixed(2);
                    value.aDiscount = +(lineTotal * (1 - discounRate)).toFixed(2);
                }
            }

            emit([key.type, key.userId, key.warehouseId, key.stockId, key.date], value);
        }
    }
}