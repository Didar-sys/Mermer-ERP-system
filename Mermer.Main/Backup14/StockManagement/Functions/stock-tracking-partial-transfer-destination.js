
                key.type = 'StockTransferDestination';
                key.warehouseId = doc.destinationWarehouseId;
                
                key.lineId = line.receivedId;
                key.lineSourceId = line.id;

                value.id = key.lineId;
                value.warehouseId = doc.destinationWarehouseId;

                var receivedActionUnit = doc.stockUnitConvertions.find(function (el) {
                    return el.stockId === line.stockId
                        && el.unitId === line.receivedUnitId;
                });
                var receivedActionQuantity = line.receivedQuantity
                    * receivedActionUnit.multiplier
                    / receivedActionUnit.divider;

                value.income = receivedActionQuantity;
                value.expense = 0;