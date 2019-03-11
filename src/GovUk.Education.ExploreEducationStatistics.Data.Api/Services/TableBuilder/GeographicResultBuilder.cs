using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class GeographicResultBuilder : IResultBuilder<IGeographicData, TableBuilderGeographicData>
    {
        public TableBuilderGeographicData BuildResult(IGeographicData data, ICollection<string> attributeFilter)
        {
            return new TableBuilderGeographicData
            {
                Year = data.Year,
                SchoolType = data.SchoolType,
                Country = data.Country,
                Region = data.Region,
                LocalAuthority = data.LocalAuthority,
                School = /*TODO DFE-163 data.School*/null,
                Attributes = attributeFilter.Count > 0
                    ? QueryUtil.FilterAttributes(data.Attributes, attributeFilter)
                    : data.Attributes
            };
        }
    }
}