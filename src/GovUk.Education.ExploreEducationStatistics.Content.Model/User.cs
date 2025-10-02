#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Identity;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class User : ICreatedTimestamp<DateTimeOffset>
{
    public const string DeletedUserPlaceholderEmail = "deleted.user@doesnotexist.com";
    public const int InviteExpiryDurationDays = 14;

    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public required string Email { get; set; }

    public Guid? DeletedById { get; set; }

    public User DeletedBy { get; set; } = null!;

    public DateTime? SoftDeleted { get; set; }

    public required bool Active { get; set; }

    public IdentityRole Role { get; set; } = null!;

    public required string RoleId { get; set; }

    public required DateTimeOffset Created { get; set; }

    public User CreatedBy { get; set; } = null!;

    public required Guid CreatedById { get; set; }

    public string DisplayName => $"{FirstName} {LastName}".Trim();

    public bool IsPendingInvite => !Active && !SoftDeleted.HasValue;

    public bool ShouldBeExpired => IsPendingInvite &&
                                Created < DateTimeOffset.UtcNow.AddDays(-InviteExpiryDurationDays);
}
