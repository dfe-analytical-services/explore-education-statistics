namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;

public record SubjectResultMetaViewModel
{
    public Dictionary<string, FilterMetaViewModel> Filters { get; init; } = [];

    public List<FootnoteViewModel> Footnotes { get; init; } = [];

    public List<IndicatorMetaViewModel> Indicators { get; init; } = [];

    public Dictionary<string, List<LocationAttributeViewModel>> Locations { get; set; } = [];

    public List<BoundaryLevelViewModel> BoundaryLevels { get; init; } = [];

    public string PublicationName { get; init; } = string.Empty;

    public string SubjectName { get; init; } = string.Empty;

    public Guid? DataSetFileId { get; init; }

    public List<TimePeriodMetaViewModel> TimePeriodRange { get; init; } = [];

    public bool GeoJsonAvailable { get; init; }

    public bool IsCroppedTable { get; init; }
}
