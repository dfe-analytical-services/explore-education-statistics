#nullable enable
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseFile
{
    public Guid Id { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public Guid ReleaseVersionId { get; set; }

    public File File { get; set; } = null!;

    public Guid FileId { get; set; }

    public string? Name { get; set; } // otherwise referred to as "title" to avoid confusion with filename

    public string? Summary { get; set; }

    public int Order { get; set; }

    public bool? PublicApiCompatible { get; set; }

    public Guid? PublicApiDataSetId { get; set; }

    public SemVersion? PublicApiDataSetVersion { get; set; }

    public string? PublicApiDataSetVersionString =>
        PublicApiDataSetVersion is not null
            ? PublicApiDataSetVersion.Patch > 0
                ? $"{PublicApiDataSetVersion.Major}.{PublicApiDataSetVersion.Minor}.{PublicApiDataSetVersion.Patch}"
                : $"{PublicApiDataSetVersion.Major}.{PublicApiDataSetVersion.Minor}"
            : null;

    public List<FilterSequenceEntry>? FilterSequence { get; set; }

    public List<IndicatorGroupSequenceEntry>? IndicatorSequence { get; set; }

    public DateTime? Published { get; set; }
}

public abstract record SequenceEntry<TEntry, TChild>(TEntry Id, List<TChild> ChildSequence);

public record FilterSequenceEntry(Guid Id, List<FilterGroupSequenceEntry> ChildSequence)
    : SequenceEntry<Guid, FilterGroupSequenceEntry>(Id, ChildSequence);

public record FilterGroupSequenceEntry(Guid Id, List<Guid> ChildSequence)
    : SequenceEntry<Guid, Guid>(Id, ChildSequence);

public record IndicatorGroupSequenceEntry(Guid Id, List<Guid> ChildSequence)
    : SequenceEntry<Guid, Guid>(Id, ChildSequence);
