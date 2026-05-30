
            const line = doc.lines[i];

            const actionUnit = doc.stockUnitConvertions.find(function (el) {
                return el.stockId === line.stockId && el.unitId === line.unitId;
            });
            const actionQuantity = line.quantity
                * actionUnit.multiplier
                / actionUnit.divider;

            const key = {
                transactionId: doc.id,

                lineId: line.id,
                lineSourceId: line.sourceId,

                date: doc.date,
                type: doc.type,
                userId: doc.userId,
                warehouseId: doc.warehouseId
            };
            const value = {
                id: line.id,
                date: doc.date,

                warehouseId: doc.warehouseId,
                stockId: line.stockId,

                income: doc.isStockIncome
                    ? actionQuantity
                    : 0,

                expense: !doc.isStockIncome
                    ? actionQuantity
                    : 0
            };