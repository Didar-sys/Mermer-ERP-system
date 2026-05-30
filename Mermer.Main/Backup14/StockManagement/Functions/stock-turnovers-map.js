function (doc, meta) {
    if (doc.id === meta.id && doc.docType && [
        'Invoice',
        'StockSlip',
        'StockTransfer'].indexOf(doc.docType) > -1 && doc.lines &&
        doc.isCompleted && !doc.isDisabled) {
        
        for (var i = 0; i < doc.lines.length; ++i) {
            const line = doc.lines[i];

            const actionUnit = doc.stockUnitConvertions.find(function (el) {
                return el.stockId === line.stockId && el.unitId === line.unitId;
            });
            const actionQuantity = line.quantity
                * actionUnit.multiplier
                / actionUnit.divider;
            
            const key = {
                warehouseId: doc.warehouseId,
                stockId: line.stockId
            }
            const value = {
                income: doc.isStockIncome
                   ? actionQuantity
                   : 0,

                expense: !doc.isStockIncome
                   ? actionQuantity
                   : 0,

                sold: 0
            };

            if (doc.docType === 'StockTransfer') {
                
                emit([key.warehouseId, key.stockId], value);

                key.warehouseId = doc.destinationWarehouseId;

                const receivedActionUnit = doc.stockUnitConvertions.find(function (el) {
                    return el.stockId === line.stockId
                        && el.unitId === line.receivedUnitId;
                });
                const receivedActionQuantity = line.receivedQuantity
                    * receivedActionUnit.multiplier
                    / receivedActionUnit.divider;

                value.income = receivedActionQuantity;
                value.expense = 0;

            } else {
                if (doc.docType === 'Invoice') {
                    if (doc.type === 'Sales') {
                        value.sold = value.expense;
                        value.expense = 0;
                    } else if (doc.type === 'SalesReturn') {
                        value.sold = -value.income;
                        value.income = 0;
                    }
                }
            }
            
            emit([key.warehouseId, key.stockId], value);
        }
    }
}