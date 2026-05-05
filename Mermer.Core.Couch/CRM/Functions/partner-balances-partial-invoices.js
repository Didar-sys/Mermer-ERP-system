
        const total = doc.actionGrandTotal;
        const payment = doc.actionPaymentsTotal - doc.actionChangesTotal;

        const key = {
            officeId: doc.officeId,
            partnerId: doc.partnerId,
            date: doc.date
        }
        const balance = {
            type: doc.type,

            debit: doc.isPartnerDebit
                ? total
                : payment,

            credit: doc.isPartnerDebit
                ? payment
                : total
        };