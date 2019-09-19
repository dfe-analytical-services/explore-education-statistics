using System.Threading.Tasks;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ISubjectService : IRepository<Subject, long>
    {
        bool IsSubjectForLatestRelease(long subjectId);

        bool Exists(ReleaseId releaseId, string name);

        Task DeleteAsync(ReleaseId releaseId, string name);
    }
}