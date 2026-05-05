
            const line = doc.lines[i];

            const key = {
                officeId: null,
                partnerId: null,
                date: doc.date
            }
            const balance = {
                type: doc.type,

                debit: 0,
                credit: 0
            };

            switch (doc.docType) {
                case 'Bill':
                    key.partnerId = doc.partnerId;
                    if (line.currencyId) {
                        var currencyConvertion = doc.currencyConvertions
                            .find(function (el) {
                                return el.currencyId === line.currencyId;
                            });

                        balance.debit = doc.isPartnerDebit && currencyConvertion
                            ? line.amount
                            * currencyConvertion.multiplier
                            / currencyConvertion.divider
                            : 0;

                        balance.credit = !doc.isPartnerDebit && currencyConvertion
                            ? line.amount
                            * currencyConvertion.multiplier
                            / currencyConvertion.divider
                            : 0;
                    }
                    break;

                case 'PartnerSlip':
                case 'PartnerTransfer':
                    key.partnerId = line.partnerId;
                    if (line.debitCurrencyId) {
                        var debitCurrencyConvertion = doc.currencyConvertions
                            .find(function (el) {
                                return el.currencyId === line.debitCurrencyId;
                            });

                        balance.debit = debitCurrencyConvertion
                            ? line.debitAmount
                            * debitCurrencyConvertion.multiplier
                            / debitCurrencyConvertion.divider
                            : 0;
                    }

                    if (line.creditCurrencyId) {
                        var creditCurrencyConvertion = doc.currencyConvertions
                            .find(function (el) {
                                return el.currencyId === line.creditCurrencyId;
                            });

                        balance.credit = creditCurrencyConvertion
                            ? line.creditAmount
                            * creditCurrencyConvertion.multiplier
                            / creditCurrencyConvertion.divider
                            : 0;
                    }

                    break;
            }

            switch (doc.docType) {
                case 'Bill':
                case 'PartnerSlip':
                    key.officeId = doc.officeId;
                    break;
                case 'PartnerTransfer':
                    key.officeId = line.officeId;
                    break;
            }