#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using ICSharpCode.SharpZipLib.Zip;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStorageUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ZipFileService : IZipFileService
    {
        private readonly IBlobStorageService _publicBlobStorageService;

        public ZipFileService(IBlobStorageService publicBlobStorageService)
        {
            _publicBlobStorageService = publicBlobStorageService;
        }

        public async Task UploadZippedFiles(IBlobContainer containerName,
            string destinationPath,
            IEnumerable<File> files,
            Guid releaseId,
            DateTime publishScheduled)
        {
            await using var memoryStream = new MemoryStream();
            await using var zipOutputStream = new ZipOutputStream(memoryStream);

            zipOutputStream.SetLevel(1);

            foreach (var file in files)
            {
                var zipEntry = new ZipEntry(file.ZipFileEntryName());
                zipOutputStream.PutNextEntry(zipEntry);

                await _publicBlobStorageService.DownloadToStream(
                    containerName: containerName,
                    path: file.PublicPath(releaseId),
                    stream: zipOutputStream);
            }

            zipOutputStream.Finish();

            await _publicBlobStorageService.UploadStream(
                containerName,
                destinationPath,
                memoryStream,
                contentType: "application/x-zip-compressed",
                metadata: GetMetaValuesReleaseDateTime(
                    releaseDateTime: publishScheduled));
        }
    }
}
