using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class TidyDataLaCharacteristic : TidyData, ITidyDataCharacteristic
    {
        public TidyDataLaCharacteristic()
        {
        }

        public TidyDataLaCharacteristic(Guid publicationId,
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
            Characteristic characteristic) :
            base(publicationId, releaseId, releaseDate, term, year, level, country, schoolType, attributes)
        {
            Region = region;
            LocalAuthority = localAuthority;
            Characteristic = characteristic;
        }

        public Region Region { get; set; }

        public LocalAuthority LocalAuthority { get; set; }

        public Characteristic Characteristic { get; set; }
    }
}