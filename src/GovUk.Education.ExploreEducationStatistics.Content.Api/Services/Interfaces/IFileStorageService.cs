using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
        IEnumerable<string> ListFiles(string publication, string release);
    }
}