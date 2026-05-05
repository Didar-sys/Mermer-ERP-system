function (doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'Stock') {
        if (doc.type)
            emit('TypeNames', doc.type);
        if (doc.group)
            emit('GroupNames', doc.group);
        if (doc.tags)
            for (var j = 0; j < doc.tags.length; ++j) {
                emit('TagNames', doc.tags[j]);
            }
        if (doc.additionalPrices)
            for (var i = 0; i < doc.additionalPrices.length; i++) {
                emit('PriceGroupNames', doc.additionalPrices[i].group);
            }

        for (var i = 0; i < doc.units.length; i++) {
            emit('UnitNames', doc.units[i].name);
        }
    }
}