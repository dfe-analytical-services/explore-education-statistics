#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class User : ICreatedTimestamp<DateTimeOffset>
{
    public const int InviteExpiryDurationDays = 14;

    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public Guid? DeletedById { get; set; }

    public User DeletedBy { get; set; } = null!;

    public DateTime? SoftDeleted { get; set; }

    public bool Active { get; set; }

    public IdentityRole Role { get; set; } = null!;

    public string RoleId { get; set; } = string.Empty;

    public DateTimeOffset Created { get; set; }

    public User CreatedBy { get; set; } = null!;

    public Guid CreatedById { get; set; }

    public string DisplayName => $"{FirstName} {LastName}";

    [NotMapped]
    public bool Expired => !Active &&
                            !SoftDeleted.HasValue &&
                            Created < DateTimeOffset.UtcNow.AddDays(-InviteExpiryDurationDays);
}
