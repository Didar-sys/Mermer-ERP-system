function (doc, meta) {
    if (!meta.id.startsWith('change:')) {
        return;
    }
  
    const index = {};
    index[doc.serverId] = [{start: doc.patchId, end: doc.patchId}];
  
    emit(doc.userId, index);
}