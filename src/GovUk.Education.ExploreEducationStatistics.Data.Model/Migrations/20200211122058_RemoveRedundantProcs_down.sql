-- Remove the new procedure & revert the table types that were dropped - note that FilterType is the latest version

DROP PROCEDURE dbo.DropAndCreateRelease;

CREATE TYPE FilterType AS TABLE
(
    Id        UNIQUEIDENTIFIER NOT NULL,
    Hint      VARCHAR(MAX),
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
