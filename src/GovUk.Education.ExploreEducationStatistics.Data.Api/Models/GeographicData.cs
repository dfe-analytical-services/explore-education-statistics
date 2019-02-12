using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class GeographicData : TidyData, IGeographicData
    {
        public GeographicData()
        {
        }

        public GeographicData(
            Guid publicationId,
            Guid releaseId,
            DateTime releaseDate,
            string term,
            int year,
            Level level,
            Country country,
            SchoolType schoolType,
            Dictionary<string, string> attributes,
            Region region,
            LocalAuthority localAuthority,
            School school) :
            base(publicationId, releaseId, releaseDate, term, year, level, country, schoolType, attributes)
        {
            Region = region;
            LocalAuthority = localAuthority;
            School = school;
        }

        public Region Region { get; set; }
        public LocalAuthority LocalAuthority { get; set; }
        public School School { get; set; }
    }
}