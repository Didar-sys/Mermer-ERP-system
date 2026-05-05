function(key, values, rereduce) {
    if (!rereduce) {
        var result = values.reduce(function (r, a) {
            r[a] = (r[a] || 0) + 1;
            return r;
        }, Object.create(null));
        return result;
    } else {
        var reresult = Object.create(null);
        for (var i = 0; i < values.length; ++i) {
            for (x in values[i]) {
                reresult[x] = (reresult[x] || 0) + values[i][x];
            }
        }
        return reresult;
    }
}