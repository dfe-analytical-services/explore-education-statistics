using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;

public class RecordPermalinkTableDownloadRequestBindingModelBuilder
{
    public RecordPermalinkTableDownloadRequestBindingModel Build() =>
        new()
        {
            PermalinkTitle = "perma link title",
            PermalinkId = Guid.NewGuid(),
            DownloadFormat = TableDownloadFormat.ODS
        };
}
