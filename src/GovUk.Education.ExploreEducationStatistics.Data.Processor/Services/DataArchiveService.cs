using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class DataArchiveService : IDataArchiveService
    {
        private readonly IFileStorageService _fileStorageService;
        
        private const string NameKey = "name";

        public DataArchiveService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task ExtractDataFiles(Guid releaseId, string zipFileName)
        {
            using (var zipBlobFileStream = new MemoryStream())
            {
                var zipBlob = _fileStorageService.GetBlobReference(AdminReleasePath(releaseId, ReleaseFileTypes.DataZip, zipFileName.ToLower()));
                await zipBlob.DownloadToStreamAsync(zipBlobFileStream);
                await zipBlob.FetchAttributesAsync();
                var name= GetName(zipBlob);
                var userName = GetUserName(zipBlob);
                
                await zipBlobFileStream.FlushAsync();
                zipBlobFileStream.Position = 0;
                using (var archive = new ZipArchive(zipBlobFileStream))
                {
                    var file1 = archive.Entries[0];
                    var file2 = archive.Entries[1];
                    var dataFile = file1.Name.Contains(".meta.") ? file2 : file1;
                    var metadataFile = file1.Name.Contains(".meta.") ? file1 : file2;
                    var dataInfo = new Dictionary<string, string>{{NameKey, name}, {MetaFileKey, metadataFile.Name.ToLower()}, {UserName, userName}};
                    var metaDataInfo = new Dictionary<string, string>{{DataFileKey, dataFile.Name.ToLower()}, {UserName, userName}};

                    await _fileStorageService.UploadFileToStorageAsync(releaseId, dataFile, ReleaseFileTypes.Data, dataInfo);
                    await _fileStorageService.UploadFileToStorageAsync(releaseId, metadataFile, ReleaseFileTypes.Metadata, metaDataInfo);
                }
            }
        }
    }
}