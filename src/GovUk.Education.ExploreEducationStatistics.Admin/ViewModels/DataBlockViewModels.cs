#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

[JsonKnownThisType("DataBlockVersionLink")]
public record DataBlockVersionViewModel : IContentBlockViewModel
{
    public Guid Id { get; init; }

    public Guid DataBlockId { get; init; }

    public List<CommentViewModel> Comments { get; init; } = new();

    public string Heading { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? DataSetName { get; set; }

    public Guid DataSetId { get; set; }

    public string? HighlightName { get; set; }

    public string? HighlightDescription { get; set; }

    public string Source { get; init; } = string.Empty;

    public FullTableQuery Query { get; init; } = null!;

    public List<IChart> Charts { get; init; } = new();

    public int Order { get; init; }

    public TableBuilderConfiguration Table { get; init; } = null!;

    public DateTimeOffset? Locked { get; init; }

    public DateTimeOffset? LockedUntil { get; init; }

    public UserDetailsViewModel? LockedBy { get; init; }
}
