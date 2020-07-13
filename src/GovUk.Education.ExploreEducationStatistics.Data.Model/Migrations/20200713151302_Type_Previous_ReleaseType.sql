CREATE TYPE ReleaseType AS TABLE
(
    Id             UNIQUEIDENTIFIER NOT NULL,
    TimeIdentifier VARCHAR(6)       NOT NULL,
    ReleaseDate    DATETIME2        NOT NULL,
    Slug           VARCHAR(MAX)     NOT NULL,
    Year           INT              NOT NULL,
    PublicationId  UNIQUEIDENTIFIER NOT NULL
);