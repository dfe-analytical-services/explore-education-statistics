#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;

public interface IAnalyticsPathResolver
{
    string BuildOutputDirectory(string[] subPaths);
}
