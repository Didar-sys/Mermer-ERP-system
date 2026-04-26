function(key, values, rereduce) {
    if (!rereduce) {
        var result = values.reduce(function (r, a) {
            var c = r.find(function (el) {
                return el.depositoryId === a.depositoryId && el.currencyId === a.currencyId;
            });
            if (!c) {
                c = {
                    depositoryId: a.depositoryId,
                    currencyId: a.currencyId,
                    income: 0,
                    expense: 0
                };
                r.push(c);
            }
            c.income += a.income;
            c.expense += a.expense;
            c[a.type] = (c[a.type] || 0) + a.income - a.expense;
            return r;
        }, []);
        return result;
    } else {
        var reresult = [];
        for (var i = 0; i < values.length; ++i) {
            reresult = values[i].reduce(function (r, a) {
                var c = r.find(function (el) {
                    return el.depositoryId === a.depositoryId && el.currencyId === a.currencyId;
                });
                if (!c) {
                    r.push(a);
                } else {
                    for (x in a) {
                        if (a.hasOwnProperty(x) && [
                            'depositoryId',
                            'currencyId'].indexOf(x) === -1) {
                            c[x] = (c[x] || 0) + a[x];
                        }
                    }
                }
                return r;
            }, reresult);
        }
        return reresult;
    }
}