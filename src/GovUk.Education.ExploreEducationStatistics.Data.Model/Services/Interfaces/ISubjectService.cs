namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ISubjectService : IRepository<Subject, long>
    {
        bool IsSubjectForLatestRelease(long subjectId);
    }
}