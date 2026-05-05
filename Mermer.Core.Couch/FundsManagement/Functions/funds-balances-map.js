function (doc, meta) {
    if (doc.id === meta.id && doc.docType) {
        if (doc.docType === 'Invoice' &&
            doc.isCompleted &&
            !doc.isDisabled) {
            if (doc.payments) {
                for (var i = 0; i < doc.payments.length; ++i) {
                    const line = doc.payments[i];

                    const currencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

                    const balance = {
                        type: doc.type,
                        depositoryId: doc.depositoryId,
                        currencyId: line.currencyId,

                        income: !doc.isFundsIncome
                            ? 0
                            : line.amount * currencyConvertion.multiplier / currencyConvertion.divider,

                        expense: doc.isFundsIncome
                            ? 0
                            : line.amount * currencyConvertion.multiplier / currencyConvertion.divider
                    };

                    const actionDate = doc.date.substring(0, 10);

                    emit(['all', 'all', actionDate], balance);
                    emit(['all', balance.currencyId, actionDate], balance);
                    emit([balance.depositoryId, 'all', actionDate], balance);
                    emit([balance.depositoryId, balance.currencyId, actionDate], balance);
                }
            }
            if (doc.changes) {
                for (var i = 0; i < doc.changes.length; ++i) {
                    const line = doc.changes[i];

                    const currencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

                    const balance = {
                        type: doc.type,
                        depositoryId: doc.depositoryId,
                        currencyId: line.currencyId,

                        income: doc.isFundsIncome
                            ? 0
                            : line.amount * currencyConvertion.multiplier / currencyConvertion.divider,

                        expense: !doc.isFundsIncome
                            ? 0
                            : line.amount * currencyConvertion.multiplier / currencyConvertion.divider
                    };

                    const actionDate = doc.date.substring(0, 10);

                    emit(['all', 'all', actionDate], balance);
                    emit(['all', balance.currencyId, actionDate], balance);
                    emit([balance.depositoryId, 'all', actionDate], balance);
                    emit([balance.depositoryId, balance.currencyId, actionDate], balance);
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
            -1 &&
            doc.isCompleted &&
            !doc.isDisabled)
            for (var i = 0; i < doc.lines.length; ++i) {
                const line = doc.lines[i];
                const balance = {
                    type: doc.type,
                    depositoryId: doc.depositoryId,
                    currencyId: line.currencyId,

                    income: 0,
                    expense: 0
                };

                if (doc.docType === 'FundsTransfer') {
                    balance.type = 'FundsTransferSource';

                    const currencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

                    balance.income = 0;
                    balance.expense = line.amount * currencyConvertion.multiplier / currencyConvertion.divider;

                    const actionDate = doc.date.substring(0, 10);

                    emit(['all', 'all', actionDate], balance);
                    emit(['all', balance.currencyId, actionDate], balance);
                    emit([balance.depositoryId, 'all', actionDate], balance);
                    emit([balance.depositoryId, balance.currencyId, actionDate], balance);

                    balance.type = 'FundsTransferDestination';
                    balance.depositoryId = doc.destinationDepositoryId;

                    const receivedCurrencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

                    balance.income = line.receivedAmount *
                        receivedCurrencyConvertion.multiplier /
                        receivedCurrencyConvertion.divider;
                    balance.expense = 0;

                    emit(['all', 'all', actionDate], balance);
                    emit(['all', balance.currencyId, actionDate], balance);
                    emit([balance.depositoryId, 'all', actionDate], balance);
                    emit([balance.depositoryId, balance.currencyId, actionDate], balance);
                } else {
                    const currencyConvertion = doc.currencyConvertions
                        .find(function(el) {
                            return el.currencyId === line.currencyId;
                        });

                    balance.income = !doc.isFundsIncome
                        ? 0
                        : line.amount * currencyConvertion.multiplier / currencyConvertion.divider;

                    balance.expense = doc.isFundsIncome
                        ? 0
                        : line.amount * currencyConvertion.multiplier / currencyConvertion.divider;

                    const actionDate = doc.date.substring(0, 10);

                    emit(['all', 'all', actionDate], balance);
                    emit(['all', balance.currencyId, actionDate], balance);
                    emit([balance.depositoryId, 'all', actionDate], balance);
                    emit([balance.depositoryId, balance.currencyId, actionDate], balance);
                }
            }
    }
}