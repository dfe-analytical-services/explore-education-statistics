#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;

public interface IReleaseFileBlobService
{
    Task<Stream> StreamBlob(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default);
}
