#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class FeaturedTable : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; } // @MarkFix remove this?

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Order { get; set; }

    public Guid DataBlockId { get; set; }

    public DataBlock DataBlock { get; set; } = null!;

    public Guid ReleaseId { get; set; }

    public Release Release { get; set; } = null!;

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }

    public Guid? CreatedById { get; set; }

    public User? CreatedBy { get; set; }

    public Guid? UpdatedById { get; set; }

    public User? UpdatedBy { get; set; }
}
