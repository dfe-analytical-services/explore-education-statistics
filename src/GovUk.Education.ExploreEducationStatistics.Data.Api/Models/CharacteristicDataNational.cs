using System;
using System.Collections.Generic;

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
            string level,
            Country country,
            string schoolType,
            Dictionary<string, string> attributes,
            Characteristic characteristic) :
            base(publicationId, releaseId, releaseDate, term, year, level, country, schoolType, attributes)
        {
            Characteristic = characteristic;
        }

        public Characteristic Characteristic { get; set; }
    }
}