/****** Object:  Index [IX_DataImports_FileId]    Script Date: 07/09/2023 14:26:25 ******/
DROP INDEX [IX_DataImports_FileId] ON [dbo].[DataImports]
GO

/****** Object:  Index [IX_DataImports_FileId]    Script Date: 07/09/2023 14:26:25 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_DataImports_FileId] ON [dbo].[DataImports]
(
	[FileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO