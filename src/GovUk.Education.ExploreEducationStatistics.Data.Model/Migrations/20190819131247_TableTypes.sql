CREATE TYPE IdListIntegerType AS TABLE
(
    id INT
);

CREATE TYPE IdListVarcharType AS TABLE
(
    id varchar(max)
);

CREATE TYPE TimePeriodListType AS TABLE
(
    year INT,
    timeIdentifier varchar(6)
);