using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class CharacteristicDataNational : CharacteristicData
    {
        public CharacteristicDataNational()
        {
        }

        public CharacteristicDataNational(Guid publicationId, int releaseId, DateTime releaseDate, string term,
            int year, Level level, Country country, SchoolType schoolType, Dictionary<string, string> attributes,
            Characteristic characteristic) : base(publicationId, releaseId, releaseDate, term, year, level, country,
            schoolType, attributes, characteristic)
        {
        }
    }
}