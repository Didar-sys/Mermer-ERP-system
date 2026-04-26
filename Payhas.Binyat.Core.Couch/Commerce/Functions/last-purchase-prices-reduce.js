function (key, values, rereduce) {
    return values.reduce(function (r, a) {          
        return r.date > a.date ? r : a;
    }, values[0]);
}