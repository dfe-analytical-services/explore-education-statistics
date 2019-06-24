namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IFileStorageService
    {
        void CopyReleaseToPublicContainer(string publication, string release);
    }
}