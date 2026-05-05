function (doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'Warehouse' && doc.tags) {
        for (var i = 0; i < doc.tags.length; ++i) {
            emit('TagNames', doc.tags[i]);
        }
    }
}