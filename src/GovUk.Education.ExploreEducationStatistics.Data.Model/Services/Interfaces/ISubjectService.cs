namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface ISubjectService : IDataService<Subject, long>
    {
        bool IsSubjectForLatestRelease(long subjectId);
    }
}