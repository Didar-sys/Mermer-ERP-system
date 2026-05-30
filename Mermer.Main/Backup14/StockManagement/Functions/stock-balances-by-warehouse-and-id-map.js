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
                stockId: line.stockId,
                date: doc.date

            }
            const value = {
                type: doc.type,

                income: doc.isStockIncome
                    ? actionQuantity
                    : 0,

                expense: !doc.isStockIncome
                    ? actionQuantity
                    : 0
            };

            if (doc.docType === 'StockTransfer') {
                value.type = 'StockTransferSource';

                emit([key.warehouseId, key.stockId, key.date], value);

                value.type = 'StockTransferDestination';
                key.warehouseId = doc.destinationWarehouseId;

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

            emit([key.warehouseId, key.stockId, key.date], value);
        }
    }
}