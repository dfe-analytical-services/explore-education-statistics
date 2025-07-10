using System;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Analytics.Dtos;

public class CapturePermaLinkTableDownloadCallTests
{
    [Fact]
    public void GivenACapturePermaLinkTableDownloadCall_WhenSerialised_ThenAllPropertiesShouldBeCaptured()
    {
        // ARRANGE
        var sut = new CapturePermaLinkTableDownloadCall
        {
            PermalinkTitle = "the permalinkTitle",
            PermalinkId = Guid.NewGuid(),
            DownloadFormat = TableDownloadFormat.ODS
        };
        
        // ACT
        var actual = AnalyticsRequestSerialiser.SerialiseRequest(sut);

        // ASSERT
        Assert.Contains(sut.PermalinkTitle, actual);
        Assert.Contains(sut.PermalinkId.ToString(), actual);
        Assert.Contains(sut.DownloadFormat.ToString(), actual);
    }
}
