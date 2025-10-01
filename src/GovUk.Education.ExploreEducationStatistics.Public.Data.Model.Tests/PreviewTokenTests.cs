using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests;

public abstract class PreviewTokenTests
{
    public class StatusTests
    {
        [Theory]
        [InlineData(PreviewTokenStatus.Active, true, false)]
        [InlineData(PreviewTokenStatus.Pending, false, false)]
        [InlineData(PreviewTokenStatus.Expired, true, true)]
        public void Status_ReturnsCorrectStatusBasedOnActivatesAndExpiry(PreviewTokenStatus expectedStatus, bool activated, bool expired)
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
        public void GetPreviewTokenStatus_ReturnsCorrectStatusBasedOnDate(PreviewTokenStatus expectedStatus, string activated, string expired)
        {
            var currentTime = DateTimeOffset.Parse("2025-10-01T00:00:00 +00:00");
            PreviewToken previewToken = new()
            {
                Label = "Test",
                DataSetVersionId = Guid.Empty,
                CreatedByUserId = Guid.Empty,
                Activates = DateTimeOffset.Parse(activated),
                Expiry = DateTimeOffset.Parse(expired)
            };
            var status = previewToken.GetPreviewTokenStatus(currentTime);
            Assert.Equal(status, expectedStatus);
        }
    }
}
