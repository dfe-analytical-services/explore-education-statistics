using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Database;

public abstract class ContentDbContextTests
{
    public class UserTests : ContentDbContextTests
    {
        public static TheoryData<bool, DateTime?, DateTimeOffset, bool> GlobalQueryFilterData => new()
    {
        // active, softDeletedDate, createdDate, expectedToBeReturned
        { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
        { false, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), true },
        { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), false },
        { false, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
        { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays - 1), true },
        { true, null, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), true },
        { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
        { true, DateTime.UtcNow, DateTimeOffset.UtcNow.AddDays(-User.InviteExpiryDurationDays + 1), false },
    };

        [Theory]
        [MemberData(nameof(GlobalQueryFilterData))]
        public async Task GlobalQueryFilter_FiltersUsersAppropriately(
            bool active,
            DateTime? softDeletedDate,
            DateTimeOffset createdDate,
            bool expectedToBeReturned)
        {
            var validUser = new User
            {
                Active = true,
                SoftDeleted = null,
                Created = DateTimeOffset.UtcNow
            };

            var otherUser = new User
            {
                Active = active,
                SoftDeleted = softDeletedDate,
                Created = createdDate
            };

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
    }
}
