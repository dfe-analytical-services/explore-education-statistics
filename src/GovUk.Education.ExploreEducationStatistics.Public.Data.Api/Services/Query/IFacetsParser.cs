using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using InterpolatedSql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;

public interface IFacetsParser
{
    public IInterpolatedSql Parse(DataSetQueryCriteriaFacets facets, string path);
}
