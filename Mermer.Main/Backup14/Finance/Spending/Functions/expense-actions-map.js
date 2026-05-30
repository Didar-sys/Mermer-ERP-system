function (doc, meta) {
    if (doc.id === meta.id && doc.docType && doc.docType === 'ExpenseSlip') {
        for (var i = 0; i < doc.lines.length; ++i) {
            const line = doc.lines[i];

            const currencyConvertion = doc.currencyConvertions
                .find(function (el) {
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

                actionDepositoryId: doc.depositoryId,
                actionExpenseId: line.expenseId,

                actionAmount: line.amount
                    * currencyConvertion.multiplier
                    / currencyConvertion.divider
            };

            const actionDate = doc.date.substring(0, 10);

            emit(['all', 'all', 'all', actionDate], action);
            emit(['all', 'all', action.actionExpenseId, actionDate], action);
            emit(['all', action.actionDepositoryId, 'all', actionDate], action);
            emit(['all', action.actionDepositoryId, action.actionExpenseId, actionDate], action);

            emit([action.transactionUserId, 'all', 'all', actionDate], action);
            emit([action.transactionUserId, 'all', action.actionExpenseId, actionDate], action);
            emit([action.transactionUserId, action.actionDepositoryId, 'all', actionDate], action);
            emit([action.transactionUserId, action.actionDepositoryId, action.actionExpenseId, actionDate], action);
        }
    }
}