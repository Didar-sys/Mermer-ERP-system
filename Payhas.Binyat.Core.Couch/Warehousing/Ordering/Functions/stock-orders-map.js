function (doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'StockOrder' &&
        doc.lines && !doc.isCompleted && !doc.isDisabled) {

        for (var i = 0; i < doc.lines.length; ++i) {

            const line = doc.lines[i];

            const actionUnit = doc.stockUnitConvertions.find(function (el) {
                return el.stockId === line.stockId && el.unitId === line.unitId;
            });
            const actionQuantity = line.quantity
                * actionUnit.multiplier
                / actionUnit.divider;
            
            const key = {
                stockId: line.stockId
            }
            const value = {
                warehouseId: doc.warehouseId,
                quantity: actionQuantity
            };

            emit(key.stockId, value);
        }
    }
}