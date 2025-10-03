using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using InterpolatedSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IDataSetQueryParser
{
    Task<IInterpolatedSql> ParseCriteria(
        IDataSetQueryCriteria criteria,
        DataSetVersion dataSetVersion,
        QueryState queryState,
        string basePath = "",
        CancellationToken cancellationToken = default
    );
}
