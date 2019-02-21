using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Meta
{
    public class CharacteristicMetaService : AbstractDataService<CharacteristicMeta>
    {
        public CharacteristicMetaService(ApplicationDbContext context) : base(context)
        {
        }

        public IEnumerable<CharacteristicMeta> Get(Guid publicationId)
        {
            return FindMany(meta => meta.PublicationId == publicationId);
        }
    }
}