create type ObservationType as table
(
    Id              uniqueidentifier not null,
    SubjectId       uniqueidentifier not null,
    LocationId      uniqueidentifier not null,
    Year            int,
    TimeIdentifier  nvarchar(6),
    Measures        nvarchar(max),
    CsvRow          bigint
);
