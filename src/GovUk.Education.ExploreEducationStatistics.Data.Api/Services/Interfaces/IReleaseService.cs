using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IReleaseService
    {
        long GetLatestRelease(Guid publicationId);

        Dictionary<string, List<AttributeMetaViewModel>> GetAttributeMetas(Guid publicationId, Type type);

        Dictionary<string, List<NameLabelViewModel>> GetCharacteristicMetas(Guid publicationId, Type type);
    }
}