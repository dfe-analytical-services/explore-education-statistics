using System;
using System.IO;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IDataGuidanceFileWriter
    {
        Task<FileStream> WriteFile(Guid releaseId, string path);
    }
}