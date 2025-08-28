#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

// EES-4640 - ideally this should be renamed to "DataBlockVersionViewModel". This will however produce a lot of
// code changes and can be deferred until DataBlockVersion has completely replaced DataBlock.
[JsonKnownThisType(nameof(DataBlock))]
public record DataBlockViewModel : IContentBlockViewModel
{
    public Guid Id { get; init; }

    // EES-4640 - this "set" can get replaced with "init" when we no longer need to manually add DataBlockParentId
    // to this ViewModel when mapped from a plain DataBlock, rather than a DataBlockVersion.
    public Guid DataBlockParentId { get; set; }

    public List<CommentViewModel> Comments { get; init; } = new();

    public string Heading { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? DataSetName { get; set; }

    public Guid DataSetId { get; set; }

    public string? HighlightName { get; set; }

    public string? HighlightDescription { get; set; }

    public string Source { get; init; }  = string.Empty;

    public FullTableQuery Query { get; init; } = null!;

    public List<IChart> Charts { get; init; } = new();

    public int Order { get; init; }

    public TableBuilderConfiguration Table { get; init; } = null!;

    public DateTimeOffset? Locked { get; init; }

    public DateTimeOffset? LockedUntil { get; init; }

    public UserDetailsViewModel? LockedBy { get; init; }
}
