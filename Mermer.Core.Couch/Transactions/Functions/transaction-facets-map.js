function(doc, meta) {
    if (doc.id === meta.id && doc.docType &&
        [
            'FundsSlip',
            'FundsTransfer',
            'DailyFundsRegistery',
            'ExpenseSlip',
            'StockSlip',
            'StockTransfer',
            'StockRevision',
            'StockOrder',
            'AggregatedStockOrder',
            'PartnerSlip',
            'PartnerTransfer',
            'Bill',
            'Invoice'
        ].indexOf(doc.docType) > -1) {
        if (doc.group)
            emit('GroupNames', doc.group);
        if (doc.tags)
            for (var i = 0; i < doc.tags.length; ++i) {
                emit('TagNames', doc.tags[i]);
            }
    }
}