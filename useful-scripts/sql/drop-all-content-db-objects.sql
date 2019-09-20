DROP TABLE __EFMigrationsHistory;
DROP TABLE Link;
DROP TABLE ReleaseSummaryVersions;
DROP TABLE [Update];
DROP TABLE ReleaseSummaries;
alter table ContentBlock
    drop constraint FK_ContentBlock_Releases_DataBlock_ReleaseId
go
alter table ContentBlock
    drop constraint FK_ContentBlock_ContentSections_ContentSectionId
go
DROP TABLE ContentSections;
DROP TABLE Releases;
DROP TABLE ReleaseTypes;
DROP TABLE Publications;
DROP TABLE Methodologies;
DROP TABLE Topics;
DROP TABLE Contacts;
DROP TABLE Themes;
DROP TABLE ContentBlock;
