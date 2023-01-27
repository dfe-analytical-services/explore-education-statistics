#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Release> Get(Guid id);

        Task<IEnumerable<Release>> List(IEnumerable<Guid> ids);

        Task<IEnumerable<Release>> GetAmendedReleases(IEnumerable<Guid> releaseIds);

        Task<List<File>> GetFiles(Guid releaseId, params FileType[] types);

        Task<Release> GetLatestRelease(Guid publicationId, IEnumerable<Guid> includedReleaseIds);

        Task SetPublishedDate(Guid releaseId, DateTime actualPublishedDate);
    }
}
