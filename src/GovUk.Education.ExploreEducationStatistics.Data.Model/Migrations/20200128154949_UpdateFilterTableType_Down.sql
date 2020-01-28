-- Revert FilterType to the version in the previous migration 20200103101609_TableTypes.sql
CREATE TYPE FilterType AS TABLE
(
    Id        UNIQUEIDENTIFIER NOT NULL,
    Hint      VARCHAR(MAX)     NOT NULL,
    Label     VARCHAR(MAX)     NOT NULL,
    Name      VARCHAR(MAX)     NOT NULL,
    SubjectId UNIQUEIDENTIFIER NOT NULL
);