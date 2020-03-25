-- Revert ReleaseType to the version in the previous migration 20200103101609_TableTypes.sql
CREATE TYPE ReleaseType AS TABLE
(
    Id            UNIQUEIDENTIFIER NOT NULL,
    Title         VARCHAR(MAX)     NOT NULL,
    ReleaseDate   DATETIME2        NOT NULL,
    Slug          VARCHAR(MAX)     NOT NULL,
    PublicationId UNIQUEIDENTIFIER NOT NULL
);