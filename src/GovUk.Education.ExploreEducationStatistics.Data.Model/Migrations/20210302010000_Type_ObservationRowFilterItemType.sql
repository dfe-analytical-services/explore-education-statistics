CREATE TYPE ObservationRowFilterItemType AS table
(
    CsvRow bigint NOT NULL,
    OldObservationId  uniqueidentifier NOT NULL,
    FilterItemId  uniqueidentifier NOT NULL
);