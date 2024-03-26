#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseFile
{
    public Guid Id { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public Guid ReleaseVersionId { get; set; }

    public File File { get; set; } = null!;

    public Guid FileId { get; set; }

    public string? Name { get; set; }

    public string? Summary { get; set; }

    public int Order { get; set; }
}
