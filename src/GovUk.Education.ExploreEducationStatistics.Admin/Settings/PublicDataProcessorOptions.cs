#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Settings;

internal class PublicDataProcessorOptions
{
    public static readonly string Section = "PublicDataProcessor";

    public string Url { get; init; } = string.Empty;
}
