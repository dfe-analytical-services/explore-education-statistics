using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        IEnumerable<FileInfo> ListFiles(string publication, string release, ReleaseFileTypes type);
    }
}