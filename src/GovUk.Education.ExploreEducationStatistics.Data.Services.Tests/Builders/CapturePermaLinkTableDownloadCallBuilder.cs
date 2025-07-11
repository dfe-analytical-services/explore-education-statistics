#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Builders;

public class CapturePermaLinkTableDownloadCallBuilder
{
    public CapturePermaLinkTableDownloadCall Build() =>
        new()
        {
            PermalinkTitle = "the permalink title",
            PermalinkId = Guid.NewGuid(),
            DownloadFormat = TableDownloadFormat.ODS
        };
}
