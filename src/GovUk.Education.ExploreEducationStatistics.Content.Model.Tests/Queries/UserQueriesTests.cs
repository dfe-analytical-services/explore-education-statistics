using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Queries;
public class UserQueriesTests
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
}
