CREATE TYPE FilterType AS TABLE
(
    Id        UNIQUEIDENTIFIER NOT NULL,
    Hint      VARCHAR(MAX)     NOT NULL,
    Label     VARCHAR(MAX)     NOT NULL,
    Name      VARCHAR(MAX)     NOT NULL,
    SubjectId UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE FilterFootnoteType AS TABLE
(
    FilterId   UNIQUEIDENTIFIER NOT NULL,
    FootnoteId UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE FilterGroupType AS TABLE
(
    Id       UNIQUEIDENTIFIER NOT NULL,
    FilterId UNIQUEIDENTIFIER NOT NULL,
    Label    VARCHAR(MAX)     NOT NULL
);

CREATE TYPE FilterGroupFootnoteType AS TABLE
(
    FilterGroupId UNIQUEIDENTIFIER NOT NULL,
    FootnoteId    UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE FilterItemType AS TABLE
(
    Id            UNIQUEIDENTIFIER NOT NULL,
    Label         VARCHAR(MAX)     NOT NULL,
    FilterGroupId UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE FilterItemFootnoteType AS TABLE
(
    FilterItemId UNIQUEIDENTIFIER NOT NULL,
    FootnoteId   UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE FootnoteType AS TABLE
(
    Id      UNIQUEIDENTIFIER NOT NULL,
    Content VARCHAR(MAX)
);

CREATE TYPE IndicatorType AS TABLE
(
    Id               UNIQUEIDENTIFIER NOT NULL,
    Label            VARCHAR(MAX)     NOT NULL,
    Name             VARCHAR(MAX)     NOT NULL,
    Unit             VARCHAR(MAX),
    IndicatorGroupId UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE IndicatorFootnoteType AS TABLE
(
    IndicatorId UNIQUEIDENTIFIER NOT NULL,
    FootnoteId  UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE IndicatorGroupType AS TABLE
(
    Id        UNIQUEIDENTIFIER NOT NULL,
    Label     VARCHAR(MAX)     NOT NULL,
    SubjectId UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE LocationType AS TABLE
(
    Id                              UNIQUEIDENTIFIER NOT NULL,
    Country_Code                    VARCHAR(MAX),
    Country_Name                    VARCHAR(MAX),
    Institution_Code                VARCHAR(MAX),
    Institution_Name                VARCHAR(MAX),
    LocalAuthority_Code             VARCHAR(MAX),
    LocalAuthority_OldCode          VARCHAR(MAX),
    LocalAuthority_Name             VARCHAR(MAX),
    LocalAuthorityDistrict_Code     VARCHAR(MAX),
    LocalAuthorityDistrict_Name     VARCHAR(MAX),
    LocalEnterprisePartnership_Code VARCHAR(MAX),
    LocalEnterprisePartnership_Name VARCHAR(MAX),
    MayoralCombinedAuthority_Code   VARCHAR(MAX),
    MayoralCombinedAuthority_Name   VARCHAR(MAX),
    MultiAcademyTrust_Code          VARCHAR(MAX),
    MultiAcademyTrust_Name          VARCHAR(MAX),
    OpportunityArea_Code            VARCHAR(MAX),
    OpportunityArea_Name            VARCHAR(MAX),
    ParliamentaryConstituency_Code  VARCHAR(MAX),
    ParliamentaryConstituency_Name  VARCHAR(MAX),
    Region_Code                     VARCHAR(MAX),
    Region_Name                     VARCHAR(MAX),
    RscRegion_Code                  VARCHAR(MAX),
    Sponsor_Code                    VARCHAR(MAX),
    Sponsor_Name                    VARCHAR(MAX),
    Ward_Code                       VARCHAR(MAX),
    Ward_Name                       VARCHAR(MAX)
);

CREATE TYPE PublicationType AS TABLE
(
    Id      UNIQUEIDENTIFIER NOT NULL,
    Title   VARCHAR(MAX)     NOT NULL,
    Slug    VARCHAR(MAX)     NOT NULL,
    TopicId UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE ReleaseType AS TABLE
(
    Id            UNIQUEIDENTIFIER NOT NULL,
    Title         VARCHAR(MAX)     NOT NULL,
    ReleaseDate   DATETIME2        NOT NULL,
    Slug          VARCHAR(MAX)     NOT NULL,
    PublicationId UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE SubjectType AS TABLE
(
    Id        UNIQUEIDENTIFIER NOT NULL,
    Name      VARCHAR(MAX)     NOT NULL,
    ReleaseId UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE SubjectFootnoteType AS TABLE
(
    SubjectId  UNIQUEIDENTIFIER NOT NULL,
    FootnoteId UNIQUEIDENTIFIER NOT NULL
);

CREATE TYPE ThemeType AS TABLE
(
    Id    UNIQUEIDENTIFIER NOT NULL,
    Title VARCHAR(MAX)     NOT NULL,
    Slug  VARCHAR(MAX)     NOT NULL
);

CREATE TYPE TopicType AS TABLE
(
    Id      UNIQUEIDENTIFIER NOT NULL,
    Title   VARCHAR(MAX)     NOT NULL,
    Slug    VARCHAR(MAX)     NOT NULL,
    ThemeId UNIQUEIDENTIFIER NOT NULL
);