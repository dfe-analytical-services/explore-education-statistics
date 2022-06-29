--
-- User permissions for the CONTENT database
--
CREATE USER [adminapp] FROM LOGIN [adminapp];
ALTER ROLE [db_ddladmin] ADD MEMBER [adminapp];
ALTER ROLE [db_datareader] ADD MEMBER [adminapp];
ALTER ROLE [db_datawriter] ADD MEMBER [adminapp];

CREATE USER [importer] FROM LOGIN [importer];
ALTER ROLE [db_ddladmin] ADD MEMBER [importer];
ALTER ROLE [db_datareader] ADD MEMBER [importer];
ALTER ROLE [db_datawriter] ADD MEMBER [importer];

CREATE USER [publisher] FROM LOGIN [publisher];
ALTER ROLE [db_ddladmin] ADD MEMBER [publisher];
ALTER ROLE [db_datareader] ADD MEMBER [publisher];
ALTER ROLE [db_datawriter] ADD MEMBER [publisher];

CREATE USER [content] FROM LOGIN [content];
GRANT SELECT ON [dbo].[Contacts] TO content;
GRANT SELECT ON [dbo].[ContentBlock] TO content;
GRANT SELECT ON [dbo].[ContentSections] TO content;
GRANT SELECT ON [dbo].[DataImports] TO content;
GRANT SELECT ON [dbo].[ExternalMethodology] TO content;
GRANT SELECT ON [dbo].[Files] TO content;
GRANT SELECT ON [dbo].[LegacyReleases] TO content;
GRANT SELECT ON [dbo].[Methodologies] TO content;
GRANT SELECT ON [dbo].[MethodologyParents] TO content;
GRANT SELECT ON [dbo].[PublicationMethodologies] TO content;
GRANT SELECT ON [dbo].[MethodologyFiles] TO content;
GRANT SELECT ON [dbo].[Publications] TO content;
GRANT SELECT ON [dbo].[ReleaseContentBlocks] TO content;
GRANT SELECT ON [dbo].[ReleaseContentSections] TO content;
GRANT SELECT ON [dbo].[ReleaseFiles] TO content;
GRANT SELECT ON [dbo].[Releases] TO content;
GRANT SELECT ON [dbo].[ReleaseTypes] TO content;
GRANT SELECT ON [dbo].[Themes] TO content;
GRANT SELECT ON [dbo].[Topics] TO content;
GRANT SELECT ON [dbo].[Update] TO content;
GRANT SELECT ON [dbo].[GlossaryEntries] TO content;
GRANT SELECT ON [dbo].[MethodologyNotes] TO content;

CREATE USER [data] FROM LOGIN [data];
GRANT SELECT ON [dbo].[Contacts] TO data;
GRANT SELECT ON [dbo].[ContentBlock] TO data;
GRANT SELECT ON [dbo].[ContentSections] TO data;
GRANT SELECT ON [dbo].[DataImports] TO data;
GRANT SELECT ON [dbo].[ExternalMethodology] TO data;
GRANT SELECT ON [dbo].[Files] TO data;
GRANT SELECT ON [dbo].[LegacyReleases] TO data;
GRANT SELECT ON [dbo].[Methodologies] TO data;
GRANT SELECT ON [dbo].[MethodologyParents] TO data;
GRANT SELECT ON [dbo].[PublicationMethodologies] TO data;
GRANT SELECT ON [dbo].[MethodologyFiles] TO data;
GRANT SELECT ON [dbo].[Publications] TO data;
GRANT SELECT ON [dbo].[ReleaseContentBlocks] TO data;
GRANT SELECT ON [dbo].[ReleaseContentSections] TO data;
GRANT SELECT ON [dbo].[ReleaseFiles] TO data;
GRANT SELECT ON [dbo].[Releases] TO data;
GRANT SELECT ON [dbo].[ReleaseTypes] TO data;
GRANT SELECT ON [dbo].[Themes] TO data;
GRANT SELECT ON [dbo].[Topics] TO data;
GRANT SELECT ON [dbo].[Update] TO data;
GRANT SELECT ON [dbo].[GlossaryEntries] TO data;
GRANT SELECT ON [dbo].[MethodologyNotes] TO data;