using Microsoft.Extensions.Time.Testing;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests;

public abstract class PreviewTokenTests
{
    public class GetPreviewTokenStatusTests : PreviewTokenTests
    {
        [Fact]
        public void WhenNowAfterExpiry_ReturnsExpired()
        {
            var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
            var token = new PreviewToken
            {
                Label = "Expired token",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Activates = now.AddHours(-3),
                Expires = now.AddHours(-2),
            };

            Assert.Equal(PreviewTokenStatus.Expired, token.GetPreviewTokenStatus(new FakeTimeProvider(now)));
        }

        [Fact]
        public void WhenRevoked_ReturnsExpired()
        {
            var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
            var token = new PreviewToken
            {
                Label = "Expired token",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Activates = now.AddDays(1), //Activates in the future
                Expires = now // Revoked just now
            };

            Assert.Equal(PreviewTokenStatus.Expired, token.GetPreviewTokenStatus(new FakeTimeProvider(now)));
        }

        [Fact]
        public void WhenNowBeforeActivates_ReturnsPending()
        {
            var now = new DateTimeOffset(2025, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
            var token = new PreviewToken
            {
                Label = "Pending token",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Activates = now.AddHours(1),
                Expires = now.AddHours(2),
            };

            Assert.Equal(PreviewTokenStatus.Pending, token.GetPreviewTokenStatus(new FakeTimeProvider()));
        }

        [Fact]
        public void AtActivates_IsActive()
        {
            var now = new DateTimeOffset(2025, 10, 01, 12, 00, 00, TimeSpan.Zero);
            var expires = now.AddHours(1);

            var token = new PreviewToken
            {
                Label = "Boundary activates",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Created = now,
                Activates = now,
                Expires = expires
            };

            Assert.Equal(PreviewTokenStatus.Active, token.GetPreviewTokenStatus(new FakeTimeProvider(now)));
        }

        [Fact]
        public void AtExpiry_IsExpired()
        {
            var now = new DateTimeOffset(2025, 10, 01, 12, 00, 00, TimeSpan.Zero);
            var activates = now.AddHours(-1);
            var expiry = now;

            var token = new PreviewToken
            {
                Label = "Boundary expiry",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Created = activates,
                Activates = activates,
                Expires = expiry,
            };

            Assert.Equal(PreviewTokenStatus.Expired, token.GetPreviewTokenStatus(new FakeTimeProvider(now)));
        }
    }
}
