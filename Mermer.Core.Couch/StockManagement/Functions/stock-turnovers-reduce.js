function (key, values, rereduce) {
    return values.reduce(function (r, a) {
        r.income += a.income;
        r.expense += a.expense;
        r.sold += a.sold;
        return r;
    });
}