﻿CREATE TYPE ObservationType AS TABLE
(
    Id              uniqueidentifier not null,
    SubjectId       uniqueidentifier not null,
    GeographicLevel nvarchar(6)      not null,
    LocationId      uniqueidentifier not null,
    Year            int,
    TimeIdentifier  nvarchar(6),
    Measures        nvarchar(max),
    CsvRow          bigint
);