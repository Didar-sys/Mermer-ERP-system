function (doc, meta) {
    if (!meta.id.startsWith('change:')) {
        return;
    }
  
    emit([doc.patch.id, doc.patchDate], doc);
}