function(doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'Invoice' && doc.type === 'Purchase') {
        for (var i = 0; i < doc.lines.length; ++i) {
            const line = doc.lines[i];
            emit([doc.warehouseId, line.stockId], {date: doc.date, stockId: line.stockId, price: line.price, currencyId: line.currencyId});
        }
    }
}