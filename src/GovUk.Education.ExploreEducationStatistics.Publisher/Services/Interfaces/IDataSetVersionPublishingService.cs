using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IDataSetVersionPublishingService
{
    Task PublishDataSetVersions(IEnumerable<Guid> releaseVersionIds);
}
