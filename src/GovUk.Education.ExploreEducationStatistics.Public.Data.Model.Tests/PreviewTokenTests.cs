namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests;

public abstract class PreviewTokenTests
{
    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
    public class StatusTests
    {
        [Theory]
        [InlineData(PreviewTokenStatus.Active, true, false)]
        [InlineData(PreviewTokenStatus.Pending, false, false)]
        [InlineData(PreviewTokenStatus.Expired, true, true)]
        public void Status_ReturnsCorrectStatusBasedOnActivatesAndExpiry(
            PreviewTokenStatus expectedStatus,
            bool activated,
            bool expired)
        {
            var past = DateTimeOffset.UtcNow.AddDays(-1);
            var future = DateTimeOffset.UtcNow.AddDays(1);
            PreviewToken previewToken = new()
            {
                Label = "Test",
                DataSetVersionId = Guid.Empty,
                CreatedByUserId = Guid.Empty,
                Activates = activated ? past : future,
                Expiry = expired ? past : future
            };
            Assert.Equal(expectedStatus, previewToken.Status);
        }

        [Theory]
        [InlineData(PreviewTokenStatus.Active, "2025-09-29T00:00:00 +00:00", "2025-10-05T00:00:00 +00:00")]
        [InlineData(PreviewTokenStatus.Pending,"2025-10-11T00:00:00 +00:00", "2025-10-15T00:00:00 +00:00")]
        [InlineData(PreviewTokenStatus.Expired,"2025-08-01T00:00:00 +00:00", "2025-08-05T00:00:00 +00:00")]
        public void GetPreviewTokenStatus_ReturnsCorrectStatusBasedOnDate(
            PreviewTokenStatus expectedStatus,
            string activated,
            string expired)
        {
            var currentTime = DateTimeOffset.Parse("2025-10-01T00:00:00 +00:00");
            var timeProvider = new TestTimeProvider(currentTime);
            PreviewToken previewToken = new()
            {
                Label = "Test",
                DataSetVersionId = Guid.Empty,
                CreatedByUserId = Guid.Empty,
                Activates = DateTimeOffset.Parse(activated),
                Expiry = DateTimeOffset.Parse(expired)
            };
            var status = previewToken.GetPreviewTokenStatus(timeProvider);
            Assert.Equal(status, expectedStatus);
        }

        [Fact]
        public void Status_ReturnsActive_WhenNowBetweenActivatesAndExpiry()
        {
            var now = DateTimeOffset.UtcNow;

            var token = new PreviewToken
            {
                Label = "Active token",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Created = now,
                Activates = now.AddHours(-1),
                Expiry = now.AddHours(1),
            };

            Assert.Equal(PreviewTokenStatus.Active, token.Status);
        }

        [Fact]
        public void Status_ReturnsPending_WhenNowBeforeActivates()
        {
            var now = DateTimeOffset.UtcNow;

            var token = new PreviewToken
            {
                Label = "Pending token",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Created = now,
                Activates = now.AddHours(1),
                Expiry = now.AddHours(2),
            };

            Assert.Equal(PreviewTokenStatus.Pending, token.Status);
        }

        [Fact]
        public void Status_ReturnsExpired_WhenNowAfterExpiry()
        {
            var now = DateTimeOffset.UtcNow;

            var token = new PreviewToken
            {
                Label = "Expired token",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Created = now,
                Activates = now.AddHours(-2),
                Expiry = now.AddHours(-1),
            };

            Assert.Equal(PreviewTokenStatus.Expired, token.Status);
        }

        [Fact]
        public void GetPreviewTokenStatus_AtActivates_IsActive()
        {
            var activates = new DateTimeOffset(2025, 10, 01, 12, 00, 00, TimeSpan.Zero);

            var token = new PreviewToken
            {
                Label = "Boundary activates",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Created = activates.AddMinutes(-10),
                Activates = activates,
                Expiry = activates.AddHours(1),
            };

            var timeProvider = new TestTimeProvider(activates);

            Assert.Equal(PreviewTokenStatus.Active, token.GetPreviewTokenStatus(timeProvider));
        }

        [Fact]
        public void GetPreviewTokenStatus_AtExpiry_IsExpired()
        {
            var activates = new DateTimeOffset(2025, 10, 01, 12, 00, 00, TimeSpan.Zero);
            var expiry = activates.AddHours(1);

            var token = new PreviewToken
            {
                Label = "Boundary expiry",
                DataSetVersionId = Guid.NewGuid(),
                CreatedByUserId = Guid.NewGuid(),
                Created = activates,
                Activates = activates,
                Expiry = expiry,
            };

            var timeProvider = new TestTimeProvider(expiry);

            Assert.Equal(PreviewTokenStatus.Expired, token.GetPreviewTokenStatus(timeProvider));
        }
    }
}
