using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Meta
{
    public class AttributeMetaService : AbstractDataService<AttributeMeta>, IAttributeMetaService
    {
        public AttributeMetaService(ApplicationDbContext context, ILogger<AttributeMetaService> logger) :
            base(context, logger)
        {
        }

        public IEnumerable<AttributeMeta> Get(Guid publicationId)
        {
            return FindMany(meta => meta.PublicationId == publicationId);
        }
    }
}