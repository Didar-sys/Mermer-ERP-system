function(doc, meta) {
    if (doc.id === meta.id && doc.docType &&
        doc.docType === 'Invoice' && doc.type === 'Sales' &&
        doc.partnerId && doc.isCompleted && !doc.isDisabled) {

        const info = {
            id: doc.id,
            code: doc.code,
            date: doc.date,
            dueDate: doc.dueDate,
            userId: doc.userId,
            userName: doc.userName,

            isCompleted : doc.isCompleted,
            isDisabled : doc.isDisabled,

            invoiceType: doc.InvoiceType,
            isPartnerDebit: doc.IsPartnerDebit,

            officeId: doc.officeId,
            warehouseId: doc.warehouseId,
            depositoryId: doc.depositoryId,
            partnerId: doc.partnerId
        };

        emit(['all', 'all', 'all', info.date], info);
        emit(['all', 'all', info.partnerId, info.date], info);
        emit(['all', info.officeId, 'all', info.date], info);
        emit(['all', info.officeId, info.partnerId, info.date], info);

        emit([info.userId, 'all', 'all', info.date], info);
        emit([info.userId, 'all', info.partnerId, info.date], info);
        emit([info.userId, info.officeId, 'all', info.date], info);
        emit([info.userId, info.officeId, info.partnerId, info.date], info);
    }
}