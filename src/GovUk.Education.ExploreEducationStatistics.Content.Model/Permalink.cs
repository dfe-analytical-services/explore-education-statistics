#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Permalink : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; init; }

    public string PublicationTitle { get; init; } = string.Empty;

    public string DataSetTitle { get; init; } = string.Empty;

    public Guid? ReleaseVersionId { get; init; }

    public Guid SubjectId { get; init; }

    public DateTime Created { get; set; }

    /// <summary>
    /// Indicates whether the permalink represents a cropped version of the originally selected data.
    /// This flag is used to warn consumers that they are only viewing a subset of the expected data.
    /// </summary>
    public bool IsCropped { get; set; }
}
