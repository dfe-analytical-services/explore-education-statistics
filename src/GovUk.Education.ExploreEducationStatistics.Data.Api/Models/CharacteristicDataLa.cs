using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class CharacteristicDataLa : CharacteristicData
    {
        public CharacteristicDataLa()
        {
        }

        public CharacteristicDataLa(Guid publicationId, int releaseId, DateTime releaseDate, string term, int year,
            Level level, Country country, SchoolType schoolType, Dictionary<string, string> attributes,
            Characteristic characteristic, Region region, LocalAuthority localAuthority) : base(publicationId,
            releaseId, releaseDate, term, year, level, country, schoolType, attributes, characteristic)
        {
            Region = region;
            LocalAuthority = localAuthority;
        }

        public Region Region { get; set; }
        
        public string RegionCode { get; set; }

        public LocalAuthority LocalAuthority { get; set; }
        
        public string LocalAuthorityCode { get; set; }
    }
}