using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests;

public class UserTests
{
    private readonly DataFixture _dataFixture = new();

    public static TheoryData<bool, DateTime?, DateTimeOffset, bool> ExpiryData =>
        new()
        {
            // active, softDeletedDate, createdDate, expectShouldExpire

            {
                false,
                null,
                DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1),
                true
            },
            {
                false,
                null,
                DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1),
                false
            },
            {
                false,
                DateTime.UtcNow,
                DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1),
                false
            },
            {
                false,
                DateTime.UtcNow,
                DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1),
                false
            },
            {
                true,
                null,
                DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1),
                false
            },
            {
                true,
                null,
                DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1),
                false
            },
            // These two cases should be impossible in reality as a soft-deleted user cannot be active. However,
            // they are included here for completeness.
            {
                true,
                DateTime.UtcNow,
                DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1),
                false
            },
            {
                true,
                DateTime.UtcNow,
                DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1),
                false
            },
        };

    [Theory]
    [MemberData(nameof(ExpiryData))]
    public void ShouldExpire(
        bool active,
        DateTime? softDeletedDate,
        DateTimeOffset createdDate,
        bool expectShouldExpire
    )
    {
        var user = _dataFixture
            .DefaultUser()
            .WithActive(active)
            .WithSoftDeleted(softDeletedDate)
            .WithCreated(createdDate)
            .Generate();

        Assert.Equal(expectShouldExpire, user.ShouldExpire);
    }
}
