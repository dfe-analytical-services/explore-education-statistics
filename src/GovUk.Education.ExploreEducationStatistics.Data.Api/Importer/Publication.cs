using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class Publication
    {
        public Guid PublicationId { get; set; }
        public AttributeMeta[] AttributeMetas { get; set; }
        public CharacteristicMeta[] CharacteristicMetas { get; set; }
        public Release[] Releases { get; set; }
    }
}