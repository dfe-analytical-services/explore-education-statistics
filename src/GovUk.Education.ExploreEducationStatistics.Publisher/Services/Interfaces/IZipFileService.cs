using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IZipFileService
    {
        Task UploadZippedFiles(
            IBlobContainer containerName,
            string destinationPath,
            IEnumerable<File> files,
            Guid releaseId,
            DateTime publishScheduled);
    }
}
