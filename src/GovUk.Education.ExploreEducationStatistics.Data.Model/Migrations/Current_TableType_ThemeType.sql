create type ThemeType as table
(
    Id    uniqueidentifier not null,
    Title varchar(max)     not null,
    Slug  varchar(max)     not null
);
