create type ObservationRowFilterItemType as table
(
    CsvRow           bigint           not null,
    OldObservationId uniqueidentifier not null,
    FilterItemId     uniqueidentifier not null
);
