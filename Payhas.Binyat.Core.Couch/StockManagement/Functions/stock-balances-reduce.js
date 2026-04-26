function (key, values, rereduce) {
    if (!rereduce) {
        var result = values.reduce(function (r, a) {
            r.income = (r.income || 0) + a.income;
            r.expense = (r.expense || 0) + a.expense;
            r[a.type] = (r[a.type] || 0) + a.income - a.expense;
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