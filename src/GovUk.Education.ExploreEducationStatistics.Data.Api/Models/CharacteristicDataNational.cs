using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class CharacteristicDataNational : TidyData, ICharacteristicData
    {
        public CharacteristicDataNational()
        {
        }

        public CharacteristicDataNational(Guid publicationId,
            Guid releaseId,
            DateTime releaseDate,
            string term,
            int year,
            Level level,
            Country country,
            SchoolType schoolType,
            Dictionary<string, string> attributes,
            Characteristic characteristic) :
            base(publicationId, releaseId, releaseDate, term, year, level, country, schoolType, attributes)
        {
            Characteristic = characteristic;
        }

        public Characteristic Characteristic { get; set; }
    }
}