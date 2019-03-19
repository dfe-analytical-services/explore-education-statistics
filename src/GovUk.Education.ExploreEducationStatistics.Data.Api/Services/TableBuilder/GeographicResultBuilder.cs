using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class GeographicResultBuilder : IResultBuilder<IGeographicSchoolData, TableBuilderGeographicData>
    {
        public TableBuilderGeographicData BuildResult(IGeographicSchoolData data, ICollection<string> indicatorFilter)
        {
            return new TableBuilderGeographicData
            {
                TimePeriod = data.TimePeriod,
                TimeIdentifier = data.TimeIdentifier,
                SchoolType = data.SchoolType,
                Country = data.Country,
                Region = data.Region,
                LocalAuthority = data.LocalAuthority,
                School = data.School,
                Indicators = indicatorFilter.Count > 0
                    ? QueryUtil.FilterIndicators(data.Indicators, indicatorFilter)
                    : data.Indicators
            };
        }
    }
}