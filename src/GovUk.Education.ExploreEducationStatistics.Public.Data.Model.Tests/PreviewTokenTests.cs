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
        public void ContainsCorrectStatusBasedOnActivatesAndExpires(PreviewTokenStatus status, bool activated, bool expired)
        {
            var past = DateTimeOffset.UtcNow.AddDays(-1);
            var future = DateTimeOffset.UtcNow.AddDays(1);
            PreviewToken previewToken = new()
            {
                Label = "Test",
                DataSetVersionId = Guid.Empty,
                CreatedByUserId = Guid.Empty,
                Activates = activated ? past : future,
                Expires = expired ? past : future
            };
            Assert.Equal(status, previewToken.Status);
        }
    }
}
