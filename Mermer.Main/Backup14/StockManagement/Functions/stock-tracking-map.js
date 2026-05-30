function (doc, meta) {
    if (doc.id === meta.id && doc.docType && [
        'Invoice',
        'StockSlip',
        'StockTransfer'].indexOf(doc.docType) > -1 &&
        doc.lines && doc.isCompleted && !doc.isDisabled) {

        for (var i = 0; i < doc.lines.length; ++i) {
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
            }
            const value = {
                warehouseId: doc.warehouseId,
                stockId: doc.stockId,

                income: doc.isStockIncome
                    ? actionQuantity
                    : 0,

                expense: !doc.isStockIncome
                    ? actionQuantity
                    : 0
            };

            if (doc.docType === 'StockTransfer') {
                key.type = 'StockTransferSource';

                emit([key.lineSourceId, key.lineId, key.transactionId, key.date], value);

                key.type = 'StockTransferDestination';
                key.warehouseId = doc.destinationWarehouseId;
                
                key.lineId = line.receivedId;
                key.lineSourceId = line.id;

                var receivedActionUnit = doc.stockUnitConvertions.find(function (el) {
                    return el.stockId === line.stockId
                        && el.unitId === line.receivedUnitId;
                });
                var receivedActionQuantity = line.receivedQuantity
                    * receivedActionUnit.multiplier
                    / receivedActionUnit.divider;

                value.income = receivedActionQuantity;
                value.expense = 0;
            }
            
            emit([key.lineSourceId, key.lineId, key.transactionId, key.date], value);
        }
    }
}