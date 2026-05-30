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
        emit(meta.id,
            {
                id : doc.id,
                code : doc.code,
                date : doc.date,
                type : doc.type,
                userId : doc.userId,
                userName : doc.userName,
                isCompleted : doc.isCompleted,
                isDisabled : doc.isDisabled,
                group : doc.group,
                tags : doc.tags,
                description : doc.description
            });
    }
}