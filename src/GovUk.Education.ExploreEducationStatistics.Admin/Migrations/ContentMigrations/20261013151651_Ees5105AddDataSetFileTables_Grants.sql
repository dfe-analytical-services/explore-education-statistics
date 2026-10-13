-- Rows are only inserted and deleted by the Admin.
-- Other services update them via DataSetFileDualWriteInterceptor when they update Files and ReleaseFiles.
GRANT SELECT, UPDATE ON [dbo].[DataSetFileVersions] TO [importer];

GRANT SELECT ON [dbo].[DataSetFiles] TO [publisher];
GRANT SELECT ON [dbo].[DataSetFileVersions] TO [publisher];
GRANT SELECT, UPDATE ON [dbo].[ReleaseVersionDataSetFileVersions] TO [publisher];

GRANT SELECT ON [dbo].[DataSetFiles] TO [public_data_processor];
GRANT SELECT ON [dbo].[DataSetFileVersions] TO [public_data_processor];
GRANT SELECT, UPDATE ON [dbo].[ReleaseVersionDataSetFileVersions] TO [public_data_processor];

GRANT SELECT ON [dbo].[DataSetFiles] TO [content];
GRANT SELECT ON [dbo].[DataSetFileVersions] TO [content];
GRANT SELECT ON [dbo].[ReleaseVersionDataSetFileVersions] TO [content];

GRANT SELECT ON [dbo].[DataSetFiles] TO [data];
GRANT SELECT ON [dbo].[DataSetFileVersions] TO [data];
GRANT SELECT ON [dbo].[ReleaseVersionDataSetFileVersions] TO [data];
