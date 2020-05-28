using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IContentService
    {
        Task UpdateAllContentAsync();
        Task UpdateContentAsync(IEnumerable<Guid> releaseIds, PublishContext context);
    }
}