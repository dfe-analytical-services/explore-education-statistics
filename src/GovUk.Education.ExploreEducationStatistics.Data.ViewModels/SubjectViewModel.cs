using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

public record SubjectViewModel
{
    public Guid Id { get; }

    public string Name { get; }

    public int Order { get; }

    public string Content { get; }

    public TimePeriodLabels TimePeriods { get; }

    public List<string> GeographicLevels { get; }

    public List<string> Filters { get; }

    public List<string> Indicators { get; }

    public FileInfo File { get; }

    public DateTime? LastUpdated { get; }

    public SubjectViewModel(
        Guid id,
        string name,
        int order,
        string content,
        TimePeriodLabels timePeriods,
        List<string> geographicLevels,
        List<string> filters,
        List<string> indicators,
        FileInfo file,
        DateTime? lastUpdated
    )
    {
        Id = id;
        Name = name;
        Order = order;
        Content = content;
        TimePeriods = timePeriods;
        GeographicLevels = geographicLevels;
        Filters = filters;
        Indicators = indicators;
        File = file;
        LastUpdated = lastUpdated;
    }
}
