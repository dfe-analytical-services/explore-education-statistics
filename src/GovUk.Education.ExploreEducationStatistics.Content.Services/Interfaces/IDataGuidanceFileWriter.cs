#nullable enable
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces
{
    public interface IDataGuidanceFileWriter
    {
        Task<FileStream> WriteFile(Release release, string destinationPath);
    }
}