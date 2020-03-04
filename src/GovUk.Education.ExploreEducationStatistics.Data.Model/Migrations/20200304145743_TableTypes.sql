CREATE TYPE ObservationType AS table
(
    Id              uniqueidentifier NOT NULL,
    SubjectId       uniqueidentifier NOT NULL,
    GeographicLevel nvarchar(6)      NOT NULL,
    LocationId      uniqueidentifier NOT NULL,
    ProviderUrn     nvarchar(450),
    SchoolLaEstab   nvarchar(450),
    Year            int,
    TimeIdentifier  nvarchar(6),
    Measures        nvarchar(MAX),
    CsvRow          bigint
);

CREATE TYPE ObservationFilterItemType AS table
(
    ObservationId uniqueidentifier NOT NULL,
    FilterItemId  uniqueidentifier NOT NULL
);

