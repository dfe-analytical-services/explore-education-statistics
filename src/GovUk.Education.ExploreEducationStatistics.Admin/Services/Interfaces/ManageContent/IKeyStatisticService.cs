#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IKeyStatisticService
{
    void CreateKeyStatisticDataBlock(KeyStatisticDataBlockCreateRequest request);

    void UpdateKeyStatisticDataBlock(KeyStatisticDataBlockUpdateRequest request);

    void Delete(Guid releaseId, Guid dataBlockId);

    void Reorder(Guid releaseId, Dictionary<Guid, int> newKeyStatisticOrder);


}
