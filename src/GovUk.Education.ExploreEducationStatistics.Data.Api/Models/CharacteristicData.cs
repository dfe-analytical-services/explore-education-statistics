using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public abstract class CharacteristicData : TidyData, ICharacteristicData
    {
        protected CharacteristicData()
        {
        }
        
        protected CharacteristicData(Guid publicationId, int releaseId, DateTime releaseDate, string term, int year,
            Level level, Country country, SchoolType schoolType, Dictionary<string, string> attributes,
            Characteristic characteristic) : base(publicationId, releaseId, releaseDate,
            term, year, level, country, schoolType, attributes)
        {
            Characteristic = characteristic;
        }

        public Characteristic Characteristic { get; set; }

        public string CharacteristicName { get; set; }
    }
}