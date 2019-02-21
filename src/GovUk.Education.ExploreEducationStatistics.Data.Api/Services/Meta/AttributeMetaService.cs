using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Meta
{
    public class AttributeMetaService : AbstractDataService<AttributeMeta>
    {
        public AttributeMetaService(ApplicationDbContext context) : base(context)
        {
        }

        public IEnumerable<AttributeMeta> Get(Guid publicationId)
        {
            return FindMany(meta => meta.PublicationId == publicationId);
        }
    }
}