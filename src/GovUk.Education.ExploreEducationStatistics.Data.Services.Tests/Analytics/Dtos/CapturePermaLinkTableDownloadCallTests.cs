using System;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;
using Snapshooter.Xunit;
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

    /// <summary>
    /// This data is processed by the Analytics Consumer.
    /// Any changes to this contract must be reflected in the corresponding calls processor.
    /// </summary>
    [Fact]
    public void GivenACapturePermaLinkTableDownloadCall_WhenSerialised_ThenOutputShouldBeBackwardsCompatible()
    {
        // ARRANGE
        var sut = new CapturePermaLinkTableDownloadCall
        {
            PermalinkTitle = "the permalinkTitle",
            PermalinkId = Guid.Parse("75e0dbb6-4348-47f2-905b-cf4c67f9c249"),
            DownloadFormat = TableDownloadFormat.ODS
        };
        
        // ACT
        var actual = AnalyticsRequestSerialiser.SerialiseRequest(sut);

        // ASSERT
        Snapshot.Match(actual);
    }
}
