
                if (doc.docType === 'Invoice') {
                    value.tIsCash = doc.isCash;
                    value.aRPId = doc.partnerId;

                    var discounRate = doc.actionTotal === 0 ? 0
                        : +(doc.actionGrandTotal / doc.actionTotal).toFixed(2);

                    value.aPrice = +(actionPrice * discounRate).toFixed(2);
                    value.aDiscount = +(lineTotal * (1 - discounRate)).toFixed(2);
                }