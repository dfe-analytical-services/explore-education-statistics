using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class GeographicResultBuilder : IResultBuilder<ITidyData, TableBuilderGeographicData>
    {
        public TableBuilderGeographicData BuildResult(ITidyData data, ICollection<string> attributeFilter)
        {
            return new TableBuilderGeographicData
            {
                Year = data.Year,
                SchoolType = data.SchoolType,
                Country = data.Country,
                Attributes = attributeFilter.Count > 0
                    ? QueryUtil.FilterAttributes(data.Attributes, attributeFilter)
                    : data.Attributes
            };
        }
    }
}