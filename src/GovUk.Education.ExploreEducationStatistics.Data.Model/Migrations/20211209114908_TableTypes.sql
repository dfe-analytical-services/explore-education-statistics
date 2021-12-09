-- Used by the Down method of migration 20211209114908 to recreate dropped table types.
create type ObservationRowFilterItemType as table
(
    CsvRow           bigint           not null,
    OldObservationId uniqueidentifier not null,
    FilterItemId     uniqueidentifier not null
);

create type ObservationRowType as table
(
    ObservationId   uniqueidentifier not null,
    SubjectId       uniqueidentifier not null,
    GeographicLevel nvarchar(6)      not null,
    LocationId      uniqueidentifier not null,
    Year            int,
    TimeIdentifier  nvarchar(6),
    Measures        nvarchar(max),
    CsvRow          bigint
);
