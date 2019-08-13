using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces
{
    public interface IFileStorageService
    {
        IEnumerable<FileInfo> ListFiles(string publication, string release, ReleaseFileTypes type);
    }
}