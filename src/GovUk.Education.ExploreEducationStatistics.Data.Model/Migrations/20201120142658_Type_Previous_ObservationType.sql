CREATE TYPE ObservationType AS TABLE
(
    Id              uniqueidentifier not null,
    SubjectId       uniqueidentifier not null,
    GeographicLevel nvarchar(6)      not null,
    LocationId      uniqueidentifier not null,
    ProviderUrn     nvarchar(450),
    SchoolLaEstab   nvarchar(450),
    Year            int,
    TimeIdentifier  nvarchar(6),
    Measures        nvarchar(max),
    CsvRow          bigint
);