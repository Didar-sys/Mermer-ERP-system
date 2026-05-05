function (keys, values, rereduce) {
    return values.reduce(function(result, index) {
            Object.keys(index).forEach(function(serverId) {
                result[serverId] = result[serverId] || [];

                result[serverId] = result[serverId]
                    .concat(index[serverId]);
                result[serverId] = result[serverId]
                    .sort(function(range1, range2) {
                        return range1.start - range2.start;
                    });

                result[serverId] = result[serverId]
                    .reduce(function(ranges, range) {
                            let last = ranges.pop();
                            if (!last) {
                                ranges.push(range);
                            } else if (last.end === range.start - 1) {
                                last.end = range.end;
                                ranges.push(last);
                            } else {
                                ranges.push(last);
                                ranges.push(range);
                            }

                            return ranges;
                        },
                        []);
            });

            return result;
        },
        {});
}