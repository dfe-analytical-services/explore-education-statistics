namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IAnalyticsPathResolver
{
    string BuildOutputDirectory(string[] subPaths);
}
