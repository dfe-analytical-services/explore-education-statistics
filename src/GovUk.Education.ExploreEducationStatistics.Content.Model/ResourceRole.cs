#nullable enable
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public abstract class ResourceRole<TRoleEnum, TResource>
    where TRoleEnum : Enum
    where TResource : class
{
    public Guid Id { get; set; }

    public User User { get; set; } = null!;

    public required Guid UserId { get; set; }

    [NotMapped]
    public TResource Resource { get; set; } = null!;

    [NotMapped]
    public Guid ResourceId { get; set; }

    public required TRoleEnum Role { get; set; }

    public DateTimeOffset? EmailSent { get; set; }

    public Guid? CreatedById { get; set; }

    public User? CreatedBy { get; set; }

    public required DateTime Created { get; set; }
}
