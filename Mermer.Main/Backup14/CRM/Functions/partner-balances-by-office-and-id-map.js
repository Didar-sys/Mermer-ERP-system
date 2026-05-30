function(doc, meta) {
    if (doc.id === meta.id) {
        if (doc.docType &&
            doc.docType === 'Invoice' &&
            doc.isCompleted &&
            !doc.isDisabled) {
            //import:partner-balances-partial-invoices.js

            emit([key.officeId, key.partnerId, key.date], balance);
            return;
        }

        if (doc.docType &&
            [
                'Bill',
                'PartnerSlip',
                'PartnerTransfer'
            ]
            .indexOf(doc.docType) >
            -1 &&
            doc.isCompleted &&
            !doc.isDisabled) {

            for (var i = 0; i < doc.lines.length; ++i) {
                //import:partner-balances-partial-others.js

                emit([key.officeId, key.partnerId, key.date], balance);
            }
        }
    }
}