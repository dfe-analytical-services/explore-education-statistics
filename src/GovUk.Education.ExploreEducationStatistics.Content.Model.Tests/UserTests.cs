using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests;

public class UserTests
{
    public static TheoryData<bool, DateTime?, DateTimeOffset, bool> ExpiryData => new()
    {
        // active, softDeletedDate, createdDate, expectedHasExpired
        { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), true },
        { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
        { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
        { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
        { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
        { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
        { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
        { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
    };

    [Theory]
    [MemberData(nameof(ExpiryData))]
    public void Expired(
        bool active, 
        DateTime? softDeletedDate, 
        DateTimeOffset createdDate, 
        bool expectedHasExpired)
    {
        var user = new User
        {
            Active = active,
            SoftDeleted = softDeletedDate,
            Created = createdDate
        };

        Assert.Equal(expectedHasExpired, user.Expired);
    }
}
