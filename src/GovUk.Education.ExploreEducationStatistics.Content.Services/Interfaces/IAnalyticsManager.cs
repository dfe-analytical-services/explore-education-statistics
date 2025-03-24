#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IAnalyticsManager
{
    void RecordReleaseVersionZipDownload(Guid releaseVersionId, IList<Guid>? fileIds = null);
}
