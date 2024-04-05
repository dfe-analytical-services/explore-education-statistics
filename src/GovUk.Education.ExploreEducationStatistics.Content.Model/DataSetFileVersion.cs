#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileVersion : ICreatedUpdatedTimestamps<DateTime?, DateTime?>
{
    public Guid Id { get; set; }

    public Guid DataSetFileId { get; set; }

    public DataSetFile DataSetFile { get; set; } = null!;

    public Guid ReleaseVersionId { get; set; }

    public Guid DataFileId { get; set; }

    public Guid MetaFileId { get; set; }

    public int Version { get; set; }

    public Guid? CreatedById { get; set; }
    public DateTime? Created { get; set; }

    public DateTime? Updated { get; set; }
}

