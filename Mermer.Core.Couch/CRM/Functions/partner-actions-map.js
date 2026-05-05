function(doc, meta) {
    if (doc.id === meta.id && doc.docType) {
        if (doc.docType === 'Invoice') {

            const total = doc.actionGrandTotal;
            const payment = doc.actionPaymentsTotal - doc.actionChangesTotal;

            const action = {
                transactionId: doc.id,
                transactionCode: doc.code,
                transactionDate: doc.date,
                transactionType: doc.type,
                transactionUserId: doc.userId,
                transactionUserName: doc.userName,

                transactionIsCompleted: doc.isCompleted,
                transactionIsDisabled: doc.isDisabled,

                transactionGroup: doc.group,
                transactionTags: doc.tags,

                actionOfficeId: doc.officeId,
                actionPartnerId: doc.partnerId,

                actionDebit: doc.isPartnerDebit
                    ? total
                    : payment,

                actionCredit: doc.isPartnerDebit
                    ? payment
                    : total
            };

            const actionDate = doc.date.substring(0, 10);

            emit(['all', 'all', 'all', 'all', actionDate], action);
            emit(['all', 'all', 'all', action.actionPartnerId, actionDate], action);
            emit(['all', 'all', action.actionOfficeId, 'all', actionDate], action);
            emit(['all', 'all', action.actionOfficeId, action.actionPartnerId, actionDate], action);

            emit(['all', action.transactionType, 'all', 'all', actionDate], action);
            emit(['all', action.transactionType, 'all', action.actionPartnerId, actionDate], action);
            emit(['all', action.transactionType, action.actionOfficeId, 'all', actionDate], action);
            emit(['all', action.transactionType, action.actionOfficeId, action.actionPartnerId, actionDate], action);

            emit([action.transactionUserId, action.transactionType, 'all', 'all', actionDate], action);
            emit([action.transactionUserId, action.transactionType, 'all', action.actionPartnerId, actionDate], action);
            emit([action.transactionUserId, action.transactionType, action.actionOfficeId, 'all', actionDate], action);
            emit([
                    action.transactionUserId, action.transactionType, action.actionOfficeId, action.actionPartnerId,
                    actionDate
                ],
                action);
            return;
        }

        if (doc.docType &&
            [
                'Bill',
                'PartnerSlip',
                'PartnerTransfer'
            ]
            .indexOf(doc.docType) >
            -1) {

            for (var i = 0; i < doc.lines.length; ++i) {

                const line = doc.lines[i];

                const action = {
                    transactionId: doc.id,
                    transactionCode: doc.code,
                    transactionDate: doc.date,
                    transactionType: doc.type,
                    transactionUserId: doc.userId,
                    transactionUserName: doc.userName,

                    transactionIsCompleted: doc.isCompleted,
                    transactionIsDisabled: doc.isDisabled,

                    transactionGroup: doc.group,
                    transactionTags: doc.tags,

                    actionOfficeId: null,
                    actionPartnerId: null,

                    actionDebit: 0,
                    actionCredit: 0
                };

                switch (doc.docType) {
                case 'Bill':
                    action.actionPartnerId = doc.partnerId;
                    if (line.currencyId) {
                        var currencyConvertion = doc.currencyConvertions
                            .find(function(el) {
                                return el.currencyId === line.currencyId;
                            });

                        action.actionDebit = doc.isPartnerDebit && currencyConvertion
                            ? line.amount * currencyConvertion.multiplier / currencyConvertion.divider
                            : 0;

                        action.actionCredit = !doc.isPartnerDebit && currencyConvertion
                            ? line.amount * currencyConvertion.multiplier / currencyConvertion.divider
                            : 0;
                    }
                    break;

                case 'PartnerSlip':
                case 'PartnerTransfer':
                    action.actionPartnerId = line.partnerId;
                    if (line.debitCurrencyId) {
                        var debitCurrencyConvertion = doc.currencyConvertions
                            .find(function(el) {
                                return el.currencyId === line.debitCurrencyId;
                            });

                        action.actionDebit = debitCurrencyConvertion
                            ? line.debitAmount * debitCurrencyConvertion.multiplier / debitCurrencyConvertion.divider
                            : 0;
                    }

                    if (line.creditCurrencyId) {
                        var creditCurrencyConvertion = doc.currencyConvertions
                            .find(function(el) {
                                return el.currencyId === line.creditCurrencyId;
                            });

                        action.actionCredit = creditCurrencyConvertion
                            ? line.creditAmount * creditCurrencyConvertion.multiplier / creditCurrencyConvertion.divider
                            : 0;
                    }

                    break;
                }

                switch (doc.docType) {
                case 'Bill':
                case 'PartnerSlip':
                    action.actionOfficeId = doc.officeId;
                    break;
                case 'PartnerTransfer':
                    action.actionOfficeId = line.officeId;
                    break;
                }

                const actionDate = doc.date.substring(0, 10);

                emit(['all', 'all', 'all', 'all', actionDate], action);
                emit(['all', 'all', 'all', action.actionPartnerId, actionDate], action);
                emit(['all', 'all', action.actionOfficeId, 'all', actionDate], action);
                emit(['all', 'all', action.actionOfficeId, action.actionPartnerId, actionDate], action);

                emit(['all', action.transactionType, 'all', 'all', actionDate], action);
                emit(['all', action.transactionType, 'all', action.actionPartnerId, actionDate], action);
                emit(['all', action.transactionType, action.actionOfficeId, 'all', actionDate], action);
                emit(['all', action.transactionType, action.actionOfficeId, action.actionPartnerId, actionDate],
                    action);

                emit([action.transactionUserId, action.transactionType, 'all', 'all', actionDate], action);
                emit([action.transactionUserId, action.transactionType, 'all', action.actionPartnerId, actionDate],
                    action);
                emit([action.transactionUserId, action.transactionType, action.actionOfficeId, 'all', actionDate],
                    action);
                emit([
                        action.transactionUserId, action.transactionType, action.actionOfficeId, action.actionPartnerId,
                        actionDate
                    ],
                    action);
            }
        }
    }
}