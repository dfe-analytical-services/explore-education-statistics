using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Extensions;
public class UserExtensionsTests
{
    private readonly DataFixture _dataFixture = new();

    public static TheoryData<bool, DateTime?, bool> PendingInviteData => new()
    {
        // active, softDeletedDate, expectIsPendingInvite
        { false, null, true },
        { false, DateTime.UtcNow, false },
        { true, null, false },
        { true, DateTime.UtcNow, false },
    };

    public static TheoryData<bool, DateTime?, DateTimeOffset, bool> ExpiryData => new()
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
        { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
        { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
    };

    [Theory]
    [MemberData(nameof(PendingInviteData))]
    public void IsPendingInvite(
        bool active,
        DateTime? softDeletedDate,
        bool expectIsPendingInvite)
    {
        var user = _dataFixture.DefaultUser()
            .WithActive(active)
            .WithSoftDeleted(softDeletedDate)
            .Generate();

        Assert.Equal(expectIsPendingInvite, user.IsPendingInvite());
    }

    [Theory]
    [MemberData(nameof(ExpiryData))]
    public void InviteHasExpired(
        bool active,
        DateTime? softDeletedDate,
        DateTimeOffset createdDate,
        bool expectInviteHasExpired)
    {
        var user = _dataFixture.DefaultUser()
            .WithActive(active)
            .WithSoftDeleted(softDeletedDate)
            .WithCreated(createdDate)
            .Generate();

        Assert.Equal(expectInviteHasExpired, user.InviteHasExpired());
    }
}
