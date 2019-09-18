using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ISubjectService : IRepository<Subject, long>
    {
        bool IsSubjectForLatestRelease(long subjectId);

        bool Exists(ReleaseId id, string name);
    }
}