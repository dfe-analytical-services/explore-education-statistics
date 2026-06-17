#nullable enable
using System.ComponentModel.DataAnnotations.Schema;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public abstract class ResourceRole<TResource> : ICreatedTimestamp<DateTimeOffset>
    where TResource : class
{
    public Guid Id { get; set; }

    public User User { get; set; } = null!;

    public required Guid UserId { get; set; }

    [NotMapped]
    public TResource Resource { get; set; } = null!;

    [NotMapped]
    public Guid ResourceId { get; set; }

    public DateTimeOffset? EmailSent { get; set; }

    public required Guid CreatedById { get; set; }

    public User CreatedBy { get; set; } = null!;

    public DateTimeOffset Created { get; set; }
}
