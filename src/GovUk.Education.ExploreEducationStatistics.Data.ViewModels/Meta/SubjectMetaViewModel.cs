namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;

public record SubjectMetaViewModel
{
    public Dictionary<string, FilterMetaViewModel> Filters { get; set; } = new();

    public Dictionary<string, IndicatorGroupMetaViewModel> Indicators { get; set; } = new();

    public Dictionary<string, LocationsMetaViewModel> Locations { get; set; } = new();

    public TimePeriodsMetaViewModel TimePeriod { get; set; } = new();
}
