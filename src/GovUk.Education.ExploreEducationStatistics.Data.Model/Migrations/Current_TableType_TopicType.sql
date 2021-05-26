create type TopicType as table
(
    Id      uniqueidentifier not null,
    Title   varchar(max)     not null,
    Slug    varchar(max)     not null,
    ThemeId uniqueidentifier not null
);
