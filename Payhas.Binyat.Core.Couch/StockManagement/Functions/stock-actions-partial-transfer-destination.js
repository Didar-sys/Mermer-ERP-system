
                key.type = 'StockTransferDestination';
                key.warehouseId = doc.destinationWarehouseId;

                value.aRWId = doc.warehouseId;
                value.aId = line.receivedId;
                value.aSourceId = line.id;
                value.aDiscount = 0;
                value.aOverhead = 0;

                var receivedActionUnit = doc.stockUnitConvertions.find(function (el) {
                    return el.stockId === line.stockId
                        && el.unitId === line.receivedUnitId;
                });
                var receivedActionQuantity = line.receivedQuantity
                    * receivedActionUnit.multiplier
                    / receivedActionUnit.divider;

                var receivedActionPrice = line.price
                    * actionCurrency.multiplier
                    / actionCurrency.divider

                    / receivedActionUnit.multiplier
                    * receivedActionUnit.divider;

                value.aIncome = receivedActionQuantity;
                value.aExpense = 0;
                value.aPrice = receivedActionPrice;