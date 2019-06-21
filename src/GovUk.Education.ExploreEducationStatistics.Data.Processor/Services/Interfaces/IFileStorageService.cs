using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<SubjectData> GetSubjectData(ImportMessage importMessage);
    }
}