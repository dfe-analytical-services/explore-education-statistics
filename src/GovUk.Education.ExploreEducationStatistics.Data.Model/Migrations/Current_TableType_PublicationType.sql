create type PublicationType as table
(
    Id      uniqueidentifier not null,
    Title   varchar(max)     not null,
    Slug    varchar(max)     not null,
    TopicId uniqueidentifier not null
);
