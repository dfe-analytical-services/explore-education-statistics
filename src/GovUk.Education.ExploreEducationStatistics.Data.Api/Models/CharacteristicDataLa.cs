using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class CharacteristicDataLa : TidyData, ICharacteristicData
    {
        public CharacteristicDataLa()
        {
        }

        public CharacteristicDataLa(Guid publicationId,
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