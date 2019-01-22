using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class TidyDataLaCharacteristic : TidyData
    {
        public TidyDataLaCharacteristic()
        {
        }

        public TidyDataLaCharacteristic(Guid publicationId,
            Guid releaseId,
            DateTime releaseDate,
            int year,
            string level,
            Country country,
            string schoolType,
            Dictionary<string, string> attributes,
            Region region,
            LocalAuthority localAuthority,
            Characteristic characteristic) :
            base(publicationId, releaseId, releaseDate, year, level, country, schoolType, attributes)
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