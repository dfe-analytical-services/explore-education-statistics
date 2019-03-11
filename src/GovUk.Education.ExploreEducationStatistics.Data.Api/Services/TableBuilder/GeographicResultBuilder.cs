using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class GeographicResultBuilder : IResultBuilder<IGeographicSchoolData, TableBuilderGeographicData>
    {
        public TableBuilderGeographicData BuildResult(IGeographicSchoolData data, ICollection<string> attributeFilter)
        {
            return new TableBuilderGeographicData
            {
                Year = data.Year,
                Term = data.Term,
                SchoolType = data.SchoolType,
                Country = data.Country,
                Region = data.Region,
                LocalAuthority = data.LocalAuthority,
                School = data.School,
                Attributes = attributeFilter.Count > 0
                    ? QueryUtil.FilterAttributes(data.Attributes, attributeFilter)
                    : data.Attributes
            };
        }
    }
}