using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Release
    {
        public long Id { get; set; }
        public DateTime ReleaseDate { get; set; }
        public Guid PublicationId { get; set; }
        public List<ReleaseIndicatorMeta> ReleaseIndicatorMetas { get; set; }
        public List<ReleaseCharacteristicMeta> ReleaseCharacteristicMetas { get; set; }

        public Release(DateTime releaseDate, Guid publicationId)
        {
            ReleaseDate = releaseDate;
            PublicationId = publicationId;
            ReleaseIndicatorMetas = new List<ReleaseIndicatorMeta>();
            ReleaseCharacteristicMetas = new List<ReleaseCharacteristicMeta>();   
        }
    }
}