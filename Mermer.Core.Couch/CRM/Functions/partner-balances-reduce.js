function (key, values, rereduce) {
    if (!rereduce) {
        var result = values.reduce(function (r, a) {
            r.debit = (r.debit || 0) + a.debit;
            r.credit = (r.credit || 0) + a.credit;
            r[a.type] = (r[a.type] || 0) + a.debit - a.credit;
            return r;
        }, Object.create(null));
        return result;
    } else {
        var reresult = Object.create(null);
        for (var i = 0; i < values.length; ++i) {
            for (var j in values[i]) {
                reresult[j] = (reresult[j] || 0) + values[i][j];
            }
        }
        return reresult;
    }
}