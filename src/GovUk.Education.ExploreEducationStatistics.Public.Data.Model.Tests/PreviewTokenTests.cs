namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests;

public abstract class PreviewTokenTests
{
    private sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
    public class StatusTests
    {
        [Fact]
        public void Status_WhenNowBetweenActivatesAndExpiry_ReturnsActive()
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
        public void Status_WhenNowBeforeActivates_ReturnsPending()
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
        public void Status_WhenNowAfterExpiry_ReturnsExpired()
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
        public void GetPreviewTokenStatus_BeforeActivates_IsPending()
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

            var timeProvider = new TestTimeProvider(activates.AddHours(-1));

            Assert.Equal(PreviewTokenStatus.Pending, token.GetPreviewTokenStatus(timeProvider));
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
