using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ITableBuilderService
    {
        TableBuilderResult GetGeographic(GeographicQueryContext query);

        TableBuilderResult GetLocalAuthority(LaQueryContext query);

        TableBuilderResult GetNational(NationalQueryContext query);
    }
}