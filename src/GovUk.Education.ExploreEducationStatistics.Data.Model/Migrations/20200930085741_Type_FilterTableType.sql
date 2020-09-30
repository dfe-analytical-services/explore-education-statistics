create type FilterTableType as table
(
    RowID INT NOT NULL,
    FilterId uniqueidentifier
)
go