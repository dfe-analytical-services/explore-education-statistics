-- Used by the Down method of migration 20211209111322 to recreate dropped table types.
create type PublicationType as table
(
    Id      uniqueidentifier not null,
    Title   varchar(max)     not null,
    Slug    varchar(max)     not null,
    TopicId uniqueidentifier not null
);

create type ThemeType as table
(
    Id    uniqueidentifier not null,
    Title varchar(max)     not null,
    Slug  varchar(max)     not null
);

create type TopicType as table
(
    Id      uniqueidentifier not null,
    Title   varchar(max)     not null,
    Slug    varchar(max)     not null,
    ThemeId uniqueidentifier not null
);
