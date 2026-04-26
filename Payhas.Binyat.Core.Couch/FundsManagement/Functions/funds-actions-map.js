function(doc, meta) {
    if (doc.id === meta.id && doc.docType) {
        if (doc.docType === 'Invoice') {
            if (doc.payments) {
                for (var i = 0; i < doc.payments.length; ++i) {
                    const line = doc.payments[i];

                    const currencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

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

                        actionRelatedPartnerId: doc.partnerId,
                        actionRelatedDepositoryId: null,

                        actionDepositoryId: doc.depositoryId,
                        actionCurrencyId: line.currencyId,
                        actionAmount: line.amount,

                        actionIncome: !doc.isFundsIncome
                            ? 0
                            : line.amount * currencyConvertion.multiplier / currencyConvertion.divider,

                        actionExpense: doc.isFundsIncome
                            ? 0
                            : line.amount * currencyConvertion.multiplier / currencyConvertion.divider
                    };

                    const actionDate = doc.date.substring(0, 10);

                    emit(['all', 'all', 'all', 'all', actionDate], action);
                    emit(['all', 'all', 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, 'all', actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, action.actionCurrencyId, actionDate], action);

                    emit(['all', action.transactionType, 'all', 'all', actionDate], action);
                    emit(['all', action.transactionType, 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', action.transactionType, action.actionDepositoryId, 'all', actionDate], action);
                    emit([
                            'all', action.transactionType, action.actionDepositoryId, action.actionCurrencyId,
                            actionDate
                        ],
                        action);

                    emit([action.transactionUserId, action.transactionType, 'all', 'all', actionDate], action);
                    emit([action.transactionUserId, action.transactionType, 'all', action.actionCurrencyId, actionDate],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId, 'all',
                            actionDate
                        ],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId,
                            action.actionCurrencyId, actionDate
                        ],
                        action);
                }
            }
            if (doc.changes) {
                for (var i = 0; i < doc.changes.length; ++i) {
                    const line = doc.changes[i];

                    const currencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

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

                        actionRelatedPartnerId: doc.partnerId,
                        actionRelatedDepositoryId: null,

                        actionDepositoryId: doc.depositoryId,
                        actionCurrencyId: line.currencyId,
                        actionAmount: line.amount,

                        actionIncome: doc.isFundsIncome
                            ? 0
                            : line.amount * currencyConvertion.multiplier / currencyConvertion.divider,

                        actionExpense: !doc.isFundsIncome
                            ? 0
                            : line.amount * currencyConvertion.multiplier / currencyConvertion.divider
                    };

                    const actionDate = doc.date.substring(0, 10);

                    emit(['all', 'all', 'all', 'all', actionDate], action);
                    emit(['all', 'all', 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, 'all', actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, action.actionCurrencyId, actionDate], action);

                    emit(['all', action.transactionType, 'all', 'all', actionDate], action);
                    emit(['all', action.transactionType, 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', action.transactionType, action.actionDepositoryId, 'all', actionDate], action);
                    emit([
                            'all', action.transactionType, action.actionDepositoryId, action.actionCurrencyId,
                            actionDate
                        ],
                        action);

                    emit([action.transactionUserId, action.transactionType, 'all', 'all', actionDate], action);
                    emit([action.transactionUserId, action.transactionType, 'all', action.actionCurrencyId, actionDate],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId, 'all',
                            actionDate
                        ],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId,
                            action.actionCurrencyId, actionDate
                        ],
                        action);
                }
            }
        }
        if (doc.docType &&
            [
                'Bill',
                'FundsSlip',
                'FundsTransfer',
                'ExpenseSlip'
            ].indexOf(doc.docType) >
            -1)
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

                    actionRelatedPartnerId: null,
                    actionRelatedDepositoryId: null,

                    actionDepositoryId: doc.depositoryId,
                    actionCurrencyId: line.currencyId,
                    actionAmount: line.amount,

                    actionIncome: 0,
                    actionExpense: 0
                };

                if (doc.docType === 'FundsTransfer') {
                    action.transactionType = 'FundsTransferSource';
                    action.actionRelatedDepositoryId = doc.destinationDepositoryId;

                    const currencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

                    action.actionIncome = 0;
                    action.actionExpense = line.amount * currencyConvertion.multiplier / currencyConvertion.divider;

                    const actionDate = doc.date.substring(0, 10);

                    emit(['all', 'all', 'all', 'all', actionDate], action);
                    emit(['all', 'all', 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, 'all', actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, action.actionCurrencyId, actionDate], action);

                    emit(['all', action.transactionType, 'all', 'all', actionDate], action);
                    emit(['all', action.transactionType, 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', action.transactionType, action.actionDepositoryId, 'all', actionDate], action);
                    emit([
                            'all', action.transactionType, action.actionDepositoryId, action.actionCurrencyId,
                            actionDate
                        ],
                        action);

                    emit([action.transactionUserId, action.transactionType, 'all', 'all', actionDate], action);
                    emit([action.transactionUserId, action.transactionType, 'all', action.actionCurrencyId, actionDate],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId, 'all',
                            actionDate
                        ],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId,
                            action.actionCurrencyId, actionDate
                        ],
                        action);

                    action.transactionType = 'FundsTransferDestination';
                    action.actionDepositoryId = doc.destinationDepositoryId;
                    action.actionRelatedDepositoryId = doc.depositoryId;

                    const receivedCurrencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

                    action.actionIncome = line.receivedAmount *
                        receivedCurrencyConvertion.multiplier /
                        receivedCurrencyConvertion.divider;
                    action.actionExpense = 0;

                    emit(['all', 'all', 'all', 'all', actionDate], action);
                    emit(['all', 'all', 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, 'all', actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, action.actionCurrencyId, actionDate], action);

                    emit(['all', action.transactionType, 'all', 'all', actionDate], action);
                    emit(['all', action.transactionType, 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', action.transactionType, action.actionDepositoryId, 'all', actionDate], action);
                    emit([
                            'all', action.transactionType, action.actionDepositoryId, action.actionCurrencyId,
                            actionDate
                        ],
                        action);

                    emit([action.transactionUserId, action.transactionType, 'all', 'all', actionDate], action);
                    emit([action.transactionUserId, action.transactionType, 'all', action.actionCurrencyId, actionDate],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId, 'all',
                            actionDate
                        ],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId,
                            action.actionCurrencyId, actionDate
                        ],
                        action);
                } else {
                    if (doc.docType === 'Bill') {
                        action.actionRelatedPartnerId = doc.partnerId;
                    }

                    const currencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

                    action.actionIncome = !doc.isFundsIncome
                        ? 0
                        : line.amount * currencyConvertion.multiplier / currencyConvertion.divider;

                    action.actionExpense = doc.isFundsIncome
                        ? 0
                        : line.amount * currencyConvertion.multiplier / currencyConvertion.divider;

                    const actionDate = doc.date.substring(0, 10);

                    emit(['all', 'all', 'all', 'all', actionDate], action);
                    emit(['all', 'all', 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, 'all', actionDate], action);
                    emit(['all', 'all', action.actionDepositoryId, action.actionCurrencyId, actionDate], action);

                    emit(['all', action.transactionType, 'all', 'all', actionDate], action);
                    emit(['all', action.transactionType, 'all', action.actionCurrencyId, actionDate], action);
                    emit(['all', action.transactionType, action.actionDepositoryId, 'all', actionDate], action);
                    emit([
                            'all', action.transactionType, action.actionDepositoryId, action.actionCurrencyId,
                            actionDate
                        ],
                        action);

                    emit([action.transactionUserId, action.transactionType, 'all', 'all', actionDate], action);
                    emit([action.transactionUserId, action.transactionType, 'all', action.actionCurrencyId, actionDate],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId, 'all',
                            actionDate
                        ],
                        action);
                    emit([
                            action.transactionUserId, action.transactionType, action.actionDepositoryId,
                            action.actionCurrencyId, actionDate
                        ],
                        action);
                }
            }
    }
}