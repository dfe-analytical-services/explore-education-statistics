#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IDataGuidanceFileService
    {
        Task<File> CreateDataGuidanceFile(Guid releaseId);
    }
}