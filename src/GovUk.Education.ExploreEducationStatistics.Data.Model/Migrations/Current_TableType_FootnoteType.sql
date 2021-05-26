create type FootnoteType as table
(
    Id      uniqueidentifier not null,
    Content varchar(max)
);
