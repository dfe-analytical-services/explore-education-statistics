create type FilterTableType as table
(
    RowID    int not null,
    FilterId uniqueidentifier
);

create type FootnoteType as table
(
    Id      uniqueidentifier not null,
    Content varchar(max)
);

create type IdListGuidType as table
(
    id uniqueidentifier
);

create type IdListIntegerType as table
(
    id int
);

create type IdListVarcharType as table
(
    id varchar(max)
);

create type LocationType as table
(
    Id                              uniqueidentifier not null,
    Country_Code                    varchar(max),
    Country_Name                    varchar(max),
    EnglishDevolvedArea_Code        varchar(max),
    EnglishDevolvedArea_Name        varchar(max),
    Institution_Code                varchar(max),
    Institution_Name                varchar(max),
    LocalAuthority_Code             varchar(max),
    LocalAuthority_OldCode          varchar(max),
    LocalAuthority_Name             varchar(max),
    LocalAuthorityDistrict_Code     varchar(max),
    LocalAuthorityDistrict_Name     varchar(max),
    LocalEnterprisePartnership_Code varchar(max),
    LocalEnterprisePartnership_Name varchar(max),
    MayoralCombinedAuthority_Code   varchar(max),
    MayoralCombinedAuthority_Name   varchar(max),
    MultiAcademyTrust_Code          varchar(max),
    MultiAcademyTrust_Name          varchar(max),
    OpportunityArea_Code            varchar(max),
    OpportunityArea_Name            varchar(max),
    ParliamentaryConstituency_Code  varchar(max),
    ParliamentaryConstituency_Name  varchar(max),
    Region_Code                     varchar(max),
    Region_Name                     varchar(max),
    RscRegion_Code                  varchar(max),
    Sponsor_Code                    varchar(max),
    Sponsor_Name                    varchar(max),
    Ward_Code                       varchar(max),
    Ward_Name                       varchar(max),
    PlanningArea_Code               varchar(max),
    PlanningArea_Name               varchar(max)
);

create type ObservationFilterItemType as table
(
    ObservationId uniqueidentifier not null,
    FilterItemId  uniqueidentifier not null,
    FilterId      uniqueidentifier not null
);

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

create type ObservationType as table
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

create type PublicationType as table
(
    Id      uniqueidentifier not null,
    Title   varchar(max)     not null,
    Slug    varchar(max)     not null,
    TopicId uniqueidentifier not null
);

create type ReleaseType as table
(
    Id                uniqueidentifier not null,
    TimeIdentifier    varchar(6)       not null,
    Slug              varchar(max)     not null,
    Year              int              not null,
    PublicationId     uniqueidentifier not null,
    PreviousVersionId uniqueidentifier
);

create type ThemeType as table
(
    Id    uniqueidentifier not null,
    Title varchar(max)     not null,
    Slug  varchar(max)     not null
);

create type TimePeriodListType as table
(
    year           int,
    timeIdentifier varchar(6)
);

create type TopicType as table
(
    Id      uniqueidentifier not null,
    Title   varchar(max)     not null,
    Slug    varchar(max)     not null,
    ThemeId uniqueidentifier not null
);
