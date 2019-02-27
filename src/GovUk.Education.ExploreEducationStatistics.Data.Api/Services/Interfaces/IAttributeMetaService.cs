using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IAttributeMetaService
    {
        IEnumerable<AttributeMeta> Get(Guid publicationId);
    }
}