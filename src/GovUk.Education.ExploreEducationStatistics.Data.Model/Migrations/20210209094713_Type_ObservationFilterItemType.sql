CREATE TYPE ObservationFilterItemType AS table
(
    CsvRow bigint NOT NULL,
    OldObservationId  uniqueidentifier NOT NULL,
    FilterItemId  uniqueidentifier NOT NULL
);