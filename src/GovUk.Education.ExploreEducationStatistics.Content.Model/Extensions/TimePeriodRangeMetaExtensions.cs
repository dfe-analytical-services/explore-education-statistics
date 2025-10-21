namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class TimePeriodRangeMetaExtensions
{
    public static (string Start, string End) ToLabels(this TimePeriodRangeMeta timePeriodRange) =>
        (timePeriodRange.Start.ToLabel(), timePeriodRange.End.ToLabel());
}
