function (doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'Stock' && doc.prices.length > 1) {

        doc.prices
            .sort(function(p1, p2) {
                return Date.parse(p1.validFrom) - Date.parse(p2.validFrom);
            })
            .reduce(function(prevPrice, nextPrice) {
                emit(nextPrice.validFrom,
                    {
                        stockId: doc.id,
                        stockCode: doc.code,
                        stockName: doc.name,

                        prevPrice,
                        nextPrice
                    });
                return nextPrice;
            });
    }
}