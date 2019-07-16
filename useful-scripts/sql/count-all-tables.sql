SELECT SCHEMA_NAME(schema_id) AS [SchemaName],
[Tables].name AS [TableName],
SUM([Partitions].[rows]) AS [TotalRowCount]
FROM sys.tables AS [Tables]
JOIN sys.partitions AS [Partitions]
ON [Tables].[object_id] = [Partitions].[object_id]
AND [Partitions].index_id IN ( 0, 1 )
WHERE [Tables].name NOT IN ('MSreplication_options', 'spt_fallback_db', 'spt_fallback_dev', 'spt_fallback_usg', 'spt_monitor')
GROUP BY SCHEMA_NAME(schema_id), [Tables].name;