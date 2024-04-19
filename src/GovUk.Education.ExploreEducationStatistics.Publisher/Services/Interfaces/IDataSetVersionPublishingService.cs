using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IDataSetVersionPublishingService
{
    Task PublishDataSetVersions(IEnumerable<Guid> releaseVersionIds);
}
