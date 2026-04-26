function (doc, meta) {
    if (doc.id === meta.id && doc.docType) {
        emit(meta.rev.substring(meta.rev.lastIndexOf('-') + 1), doc.docType);
    }
}