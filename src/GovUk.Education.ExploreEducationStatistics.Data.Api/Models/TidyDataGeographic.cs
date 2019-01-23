using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class TidyDataGeographic : TidyData
    {
        public TidyDataGeographic()
        {
        }

        public TidyDataGeographic(
            Guid publicationId,
            Guid releaseId,
            DateTime releaseDate,
            string term,
            int year,
            string level,
            Country country,
            string schoolType,
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