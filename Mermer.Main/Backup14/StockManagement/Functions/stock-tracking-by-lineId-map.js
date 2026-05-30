function (doc, meta) {
    if (doc.id === meta.id && doc.docType && [
        'Invoice',
        'StockSlip',
        'StockTransfer'].indexOf(doc.docType) > -1 &&
        doc.lines && doc.isCompleted && !doc.isDisabled) {

        for (var i = 0; i < doc.lines.length; ++i) {
            //import:stock-tracking-partial-consts.js

            if (doc.docType === 'StockTransfer') {
                //import:stock-tracking-partial-transfer-source.js

                emit(key.lineId, value);
                //import:stock-tracking-partial-transfer-destination.js
            }
            
            emit(key.lineId, value);
        }
    }
}