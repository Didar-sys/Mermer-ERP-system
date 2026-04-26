
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