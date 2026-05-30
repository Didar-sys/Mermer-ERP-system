function(doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'Invoice') {
        
        const info = {
            id : doc.id,
            date: doc.date,
            type: doc.type,
            code : doc.code,
            userId: doc.userId,
            userName: doc.userName,

            isCash: doc.isCash,
            isCompleted : doc.isCompleted,
            isDisabled : doc.isDisabled,

            group: doc.group,
            tags: doc.tags,

            officeId: doc.officeId,
            warehouseId: doc.warehouseId,
            depositoryId: doc.depositoryId,
            partnerId: doc.partnerId,

            actionTotal: doc.actionTotal,
            actionDiscountsTotal: doc.actionDiscountsTotal,
            actionGrandTotal: doc.actionGrandTotal
        };

        emit(['all', 'all', 'all', info.date], info);
        emit(['all', 'all', info.officeId, info.date], info);

        emit([info.type, 'all', 'all', info.date], info);
        emit([info.type, 'all', info.officeId, info.date], info);

        emit([info.type, info.userId, 'all', info.date], info);
        emit([info.type, info.userId, info.officeId, info.date], info);
    }
}