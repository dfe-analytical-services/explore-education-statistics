using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions;

public class UserExtensionsTests
{
    private readonly DataFixture _dataFixture = new();

    public static TheoryData<bool, DateTime?, DateTimeOffset, bool> IsInvitePendingData =>
        new()
        {
            // active, softDeletedDate, createdDate, expectIsPendingInvite

            { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
            { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), true },
            { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
            { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
            { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
            { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
            // These two cases should be impossible in reality as a soft-deleted user cannot be active. However,
            // they are included here for completeness.
            { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
            { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
        };

    public static TheoryData<bool, DateTime?, DateTimeOffset, bool> IsInviteExpiredData =>
        new()
        {
            // active, softDeletedDate, createdDate, expectInviteHasExpired

            { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), true },
            { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
            { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
            { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
            { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
            { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
            // These two cases should be impossible in reality as a soft-deleted user cannot be active. However,
            // they are included here for completeness.
            { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
            { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
        };

    [Theory]
    [MemberData(nameof(IsInvitePendingData))]
    public void IsInvitePending(
        bool active,
        DateTime? softDeletedDate,
        DateTimeOffset createdDate,
        bool expectIsPendingInvite
    )
    {
        User user = _dataFixture
            .DefaultUser()
            .WithActive(active)
            .WithSoftDeleted(softDeletedDate)
            .WithCreated(createdDate);

        Assert.Equal(expectIsPendingInvite, user.IsInvitePending());
    }

    [Theory]
    [MemberData(nameof(IsInviteExpiredData))]
    public void IsInviteExpired(
        bool active,
        DateTime? softDeletedDate,
        DateTimeOffset createdDate,
        bool expectInviteHasExpired
    )
    {
        User user = _dataFixture
            .DefaultUser()
            .WithActive(active)
            .WithSoftDeleted(softDeletedDate)
            .WithCreated(createdDate);

        Assert.Equal(expectInviteHasExpired, user.IsInviteExpired());
    }
}
