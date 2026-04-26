function (doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'Partner') {
        if (doc.group)
            emit('GroupNames', doc.group);
        if (doc.tags)
            for (var i = 0; i < doc.tags.length; ++i) {
                emit('TagNames', doc.tags[i]);
            }
    }
}