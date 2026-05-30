function (doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'Stock') {
        emit(meta.id,
            {
                id: doc.id,
                code: doc.code,
                name: doc.name,
                shortName: doc.shortName,
                isDisabled: doc.isDisabled,

                unit: doc.unit,
                price: doc.price,
                currencyId: doc.currencyId,

                type: doc.type,
                group: doc.group,
                tags: doc.tags,
                barcodes: doc.barcodes
            });
    }
}