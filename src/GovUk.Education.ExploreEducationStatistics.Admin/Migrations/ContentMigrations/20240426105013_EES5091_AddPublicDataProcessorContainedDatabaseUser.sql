IF NOT EXISTS (SELECT name
                FROM [sys].[database_principals]
                WHERE name = 'public_data_processor')
BEGIN
    CREATE USER [public_data_processor] FROM LOGIN [public_data_processor];

    GRANT ALL ON [dbo].[Comment] TO [public_data_processor];
    GRANT ALL ON [dbo].[Contacts] TO [public_data_processor];
    GRANT ALL ON [dbo].[ContentBlock] TO [public_data_processor];
    GRANT ALL ON [dbo].[ContentSections] TO [public_data_processor];
    GRANT ALL ON [dbo].[DataBlocks] TO [public_data_processor];
    GRANT ALL ON [dbo].[DataBlockVersions] TO [public_data_processor];
    GRANT ALL ON [dbo].[DataImportErrors] TO [public_data_processor];
    GRANT ALL ON [dbo].[DataImports] TO [public_data_processor];
    GRANT ALL ON [dbo].[EmbedBlocks] TO [public_data_processor];
    GRANT ALL ON [dbo].[ExternalMethodology] TO [public_data_processor];
    GRANT ALL ON [dbo].[FeaturedTables] TO [public_data_processor];
    GRANT ALL ON [dbo].[Files] TO [public_data_processor];
    GRANT ALL ON [dbo].[GlossaryEntries] TO [public_data_processor];
    GRANT ALL ON [dbo].[KeyStatistics] TO [public_data_processor];
    GRANT ALL ON [dbo].[KeyStatisticsDataBlock] TO [public_data_processor];
    GRANT ALL ON [dbo].[KeyStatisticsText] TO [public_data_processor];
    GRANT ALL ON [dbo].[Methodologies] TO [public_data_processor];
    GRANT ALL ON [dbo].[MethodologyFiles] TO [public_data_processor];
    GRANT ALL ON [dbo].[MethodologyNotes] TO [public_data_processor];
    GRANT ALL ON [dbo].[MethodologyRedirects] TO [public_data_processor];
    GRANT ALL ON [dbo].[MethodologyStatus] TO [public_data_processor];
    GRANT ALL ON [dbo].[MethodologyVersions] TO [public_data_processor];
    GRANT ALL ON [dbo].[Permalinks] TO [public_data_processor];
    GRANT ALL ON [dbo].[PublicationMethodologies] TO [public_data_processor];
    GRANT ALL ON [dbo].[PublicationRedirects] TO [public_data_processor];
    GRANT ALL ON [dbo].[Publications] TO [public_data_processor];
    GRANT ALL ON [dbo].[ReleaseFiles] TO [public_data_processor];
    GRANT ALL ON [dbo].[Releases] TO [public_data_processor];
    GRANT ALL ON [dbo].[ReleaseStatus] TO [public_data_processor];
    GRANT ALL ON [dbo].[ReleaseVersions] TO [public_data_processor];
    GRANT ALL ON [dbo].[Themes] TO [public_data_processor];
    GRANT ALL ON [dbo].[Topics] TO [public_data_processor];
    GRANT ALL ON [dbo].[Update] TO [public_data_processor];
    GRANT ALL ON [dbo].[UserInvites] TO [public_data_processor];
    GRANT ALL ON [dbo].[UserPublicationInvites] TO [public_data_processor];
    GRANT ALL ON [dbo].[UserPublicationRoles] TO [public_data_processor];
    GRANT ALL ON [dbo].[UserReleaseInvites] TO [public_data_processor];
    GRANT ALL ON [dbo].[UserReleaseRoles] TO [public_data_processor];
    GRANT ALL ON [dbo].[Users] TO [public_data_processor];
END
