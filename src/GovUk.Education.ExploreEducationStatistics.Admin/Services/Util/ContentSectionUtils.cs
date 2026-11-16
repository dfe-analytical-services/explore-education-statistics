#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Util;

public static class ContentSectionUtils
{
    public static void InitialiseBaseContentSections(ReleaseVersion releaseVersion)
    {
        releaseVersion.HeadlinesSection = CreateContentSection(ContentSectionType.Headlines);
        releaseVersion.KeyStatisticsSecondarySection = CreateContentSection(ContentSectionType.KeyStatisticsSecondary);
        releaseVersion.RelatedDashboardsSection = CreateContentSection(ContentSectionType.RelatedDashboards);
        releaseVersion.SummarySection = CreateContentSection(ContentSectionType.ReleaseSummary);
        releaseVersion.WarningSection = CreateContentSection(ContentSectionType.Warning);
    }

    public static ContentSection CreateContentSection(ContentSectionType type) => new() { Type = type };
}
