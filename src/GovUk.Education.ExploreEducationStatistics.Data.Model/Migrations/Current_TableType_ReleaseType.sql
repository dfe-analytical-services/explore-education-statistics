create type ReleaseType as table
(
    Id                uniqueidentifier not null,
    TimeIdentifier    varchar(6)       not null,
    Slug              varchar(max)     not null,
    Year              int              not null,
    PublicationId     uniqueidentifier not null,
    PreviousVersionId uniqueidentifier
);
