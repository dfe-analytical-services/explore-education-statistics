using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class Release
    {
        public long Id { get; set; }
        public DateTime ReleaseDate { get; set; }
        public Guid PublicationId { get; set; }
        public List<ReleaseAttributeMeta> ReleaseAttributeMetas { get; set; }
        public List<ReleaseCharacteristicMeta> ReleaseCharacteristicMetas { get; set; }

        public Release(DateTime releaseDate, Guid publicationId)
        {
            ReleaseDate = releaseDate;
            PublicationId = publicationId;
            ReleaseAttributeMetas = new List<ReleaseAttributeMeta>();
            ReleaseCharacteristicMetas = new List<ReleaseCharacteristicMeta>();   
        }
    }
}