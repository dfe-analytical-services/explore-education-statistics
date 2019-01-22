using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class TidyDataNationalCharacteristic : TidyData
    {
        public TidyDataNationalCharacteristic()
        {
        }

        public TidyDataNationalCharacteristic(Guid publicationId,
            Guid releaseId,
            DateTime releaseDate,
            int year,
            string level,
            Country country,
            string schoolType,
            Dictionary<string, string> attributes,
            Characteristic characteristic) :
            base(publicationId, releaseId, releaseDate, year, level, country, schoolType, attributes)
        {
            Characteristic = characteristic;
        }

        public Characteristic Characteristic { get; set; }
    }
}