using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Database;

public abstract class ContentDbContextTests
{
    private readonly DataFixture _dataFixture = new();

    public class UserTests : ContentDbContextTests
    {
        public static TheoryData<bool, DateTime?, DateTimeOffset, bool> GlobalQueryFilterData => new()
    {
        // active, softDeletedDate, createdDate, expectedToBeReturned

        // This is the only case where the user has genuinely expired and should be filtered out
        { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
        { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), true },
        { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), true },
        { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), true },
        { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), true },
        { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), true },
        { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), true },
        { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), true },
    };

        [Theory]
        [MemberData(nameof(GlobalQueryFilterData))]
        public async Task GlobalQueryFilter_FiltersUsersAppropriately(
            bool active,
            DateTime? softDeletedDate,
            DateTimeOffset createdDate,
            bool expectedToBeReturned)
        {
            var validUser = _dataFixture.DefaultUser()
                .Generate();

            var otherUser = _dataFixture.DefaultUser()
                .WithActive(active)
                .WithSoftDeleted(softDeletedDate)
                .WithCreated(createdDate)
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Users.AddRange(validUser, otherUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var users = await contentDbContext.Users.ToListAsync();

                Assert.Contains(users, u => u.Id == validUser.Id);

                if (expectedToBeReturned)
                {
                    Assert.Contains(users, u => u.Id == otherUser.Id);
                    Assert.Equal(2, users.Count);
                }
                else
                {
                    Assert.DoesNotContain(users, u => u.Id == otherUser.Id);
                    Assert.Single(users);
                }
            }
        }

        [Fact]
        public async Task GlobalQueryFilter_DoesNotFilterPlaceholderDeletedUser()
        {
            var placeholderDeletedUser = _dataFixture.DefaultUser()
                .WithEmail(User.DeletedUserPlaceholderEmail)
                .WithActive(false)
                .WithSoftDeleted(DateTime.UtcNow)
                .WithCreated(DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1))
                .Generate();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(placeholderDeletedUser);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var user = await contentDbContext.Users.SingleAsync();

                Assert.Equal(User.DeletedUserPlaceholderEmail, user.Email);
            }
        }
    }
}
