using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Builders;

public class CaptureTableToolDownloadCallBuilder
{
    private FullTableQuery? _fullTableQuery;

    public CaptureTableToolDownloadCall Build() =>
        new()
        {
            ReleaseVersionId = Guid.NewGuid(),
            PublicationName = "the publication name",
            ReleasePeriodAndLabel = "the release period and label",
            SubjectId = Guid.NewGuid(),
            DataSetName = "the data set name",
            DownloadFormat = TableDownloadFormat.ODS,
            Query = _fullTableQuery ?? new FullTableQueryBuilder().Build() 
        };
    
    public CaptureTableToolDownloadCallBuilder WhereQueryIs(FullTableQuery query)
    {
        _fullTableQuery = query;
        return this;
    }
}