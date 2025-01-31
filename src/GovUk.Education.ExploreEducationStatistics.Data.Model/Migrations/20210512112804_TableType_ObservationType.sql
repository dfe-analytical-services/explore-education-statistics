/* 
   This SQL file wasn't used in the 20210512112804 migration.
   It was created in case we need to revert the 20220325160924 migration.
   The original ObservationType was created as part of 20210512112804_TableTypes.sql 
 */
create type ObservationType as table
(
    Id              uniqueidentifier not null,
    SubjectId       uniqueidentifier not null,
    GeographicLevel nvarchar(6)      not null,
    LocationId      uniqueidentifier not null,
    Year            int,
    TimeIdentifier  nvarchar(6),
    Measures        nvarchar(max),
    CsvRow          bigint
);
