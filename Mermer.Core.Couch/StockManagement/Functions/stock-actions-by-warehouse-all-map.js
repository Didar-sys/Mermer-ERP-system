function (doc, meta) {
    if (doc.id === meta.id && doc.docType && [
        'Invoice',
        'StockSlip',
        'StockTransfer'].indexOf(doc.docType) > -1 &&
        doc.lines) {
        stock-actions-partial-overheads.js

        for (var i = 0; i < doc.lines.length; ++i) {
            stock-actions-partial-consts.js

            if (doc.docType === 'StockTransfer') {
                stock-actions-partial-transfer-source.js

                emit([key.type, key.warehouseId, key.date, key.stockId, key.userId], value);

                stock-actions-partial-transfer-destination.js

            } else {
                stock-actions-partial-others.js
            }

            emit([key.type, key.warehouseId, key.date, key.stockId, key.userId], value);
        }
    }
}