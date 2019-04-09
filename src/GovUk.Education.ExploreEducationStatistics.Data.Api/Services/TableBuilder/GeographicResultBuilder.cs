using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class GeographicResultBuilder : IResultBuilder<ISchoolData, TableBuilderGeographicData>
    {
        public TableBuilderGeographicData BuildResult(ISchoolData data, ICollection<string> indicatorFilter)
        {
            return new TableBuilderGeographicData
            {
                TimePeriod = data.TimePeriod,
                TimeIdentifier = data.TimeIdentifier,
                SchoolType = data.SchoolType,
                Country = data.Level.Country,
                Region = data.Level.Region,
                LocalAuthority = data.Level.LocalAuthority,
                School = data.School,
                Indicators = indicatorFilter.Count > 0
                    ? QueryUtil.FilterIndicators(data.Indicators, indicatorFilter)
                    : data.Indicators
            };
        }
    }
}