UPDATE ofi
SET ofi.FilterId = fg.FilterId
FROM ObservationFilterItem ofi
JOIN FilterItem fi on FilterItemId = fi.Id
JOIN FilterGroup fg ON fi.FilterGroupId = fg.Id