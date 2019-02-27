using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface ICharacteristicMetaService
    {
        IEnumerable<CharacteristicMeta> Get(Guid publicationId);
    }
}