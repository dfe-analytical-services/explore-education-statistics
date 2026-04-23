/**
  Table-valued function which can be used to perform a full-text search on the Publications free-text indexed columns.
  This can be mapped to a queryable function in EF Core.
 */
CREATE OR ALTER FUNCTION dbo.PublicationsFreeTextTable(@search nvarchar(4000))
RETURNS TABLE
AS
RETURN
(
    SELECT [Key] AS Id, Rank
    FROM FREETEXTTABLE(
    dbo.Publications,
    (Title, Summary),
    @search)
);
