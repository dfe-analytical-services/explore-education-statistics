#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IMethodologyService
    {
        Task<MethodologyVersion> Get(Guid methodologyVersionId);

        Task<List<MethodologyVersion>> GetLatestVersionByRelease(Release release);

        Task<List<File>> GetFiles(Guid methodologyVersionId, params FileType[] types);

        Task Publish(MethodologyVersion methodologyVersion);

        Task<bool> IsBeingPublishedAlongsideRelease(MethodologyVersion methodologyVersion, Release release);
    }
}
