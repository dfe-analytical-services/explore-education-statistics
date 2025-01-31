CREATE TYPE ModifiedTablesType AS TABLE
(
    Id              INT IDENTITY(1,1),
    TableName       VARCHAR(128) NOT NULL
);
