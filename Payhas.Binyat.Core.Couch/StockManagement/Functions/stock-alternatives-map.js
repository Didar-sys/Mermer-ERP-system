function (doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'StockAlternative' && doc.lines) {
        for (var i = 0; i < doc.lines.length; ++i) {
            for (var j = 0; j < doc.lines.length; ++j) {
                if (doc.lines[i].stockId !== doc.lines[j].stockId)
                    emit(doc.lines[i].stockId, doc.lines[j].stockId);
            }
        }
    }
}