IF OBJECT_ID(N'__Log_RebuildIndexes', N'U') IS NULL
CREATE TABLE __Log_RebuildIndexes
(
    Id         INT IDENTITY (1,1),
    StartTime  DATETIME2 NOT NULL,
    EndTime    DATETIME2,
    HitTimeout BIT,
    PRIMARY KEY (Id),
);

IF OBJECT_ID(N'__Log_RebuildIndexesAlterIndexes', N'U') IS NULL
CREATE TABLE __Log_RebuildIndexesAlterIndexes
(
    Id               INT IDENTITY (1,1),
    RunId            INT NOT NULL,
    IndexName        NVARCHAR(MAX) NOT NULL,
    SchemaName       NVARCHAR(MAX) NOT NULL,
    ObjectName       NVARCHAR(MAX) NOT NULL,
    StartTime        DATETIME2,
    EndTime          DATETIME2,
    StartFragPercent FLOAT,
    ActionRequired   NVARCHAR(MAX),
    HitTimeout       BIT DEFAULT 0,
    PRIMARY KEY (Id),
    FOREIGN KEY (RunId) REFERENCES __Log_RebuildIndexes (Id),
);