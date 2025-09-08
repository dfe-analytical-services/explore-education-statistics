using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;

public class RecordTableToolDownloadRequestBindingModelBuilder
{
    public RecordTableToolDownloadRequestBindingModel Build() =>
        new()
        {
            DataSetName = "data set name",
            DownloadFormat = TableDownloadFormat.ODS,
            PublicationName = "Publication Name",
            ReleasePeriodAndLabel = "release period label",
            ReleaseVersionId = Guid.NewGuid(),
            SubjectId = Guid.NewGuid(),
            Query = new FullTableQueryRequest
            {
                SubjectId = Guid.NewGuid(),
                Filters = [Guid.NewGuid(), Guid.NewGuid()],
                Indicators = [Guid.NewGuid(), Guid.NewGuid()],
                LocationIds = [Guid.NewGuid(), Guid.NewGuid()],
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2025,
                    StartCode = TimeIdentifier.July,
                    EndYear = 2026,
                    EndCode = TimeIdentifier.November,
                },
                FilterHierarchiesOptions = new Dictionary<Guid, List<FilterHierarchyOption>>
                {
                    { Guid.NewGuid(), [new FilterHierarchyOption([Guid.NewGuid(), Guid.NewGuid()])] },
                    { Guid.NewGuid(), [new FilterHierarchyOption([Guid.NewGuid(), Guid.NewGuid()])] },
                },
            },
        };
}
