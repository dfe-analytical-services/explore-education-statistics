#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.IBlobStorageService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau
{
    [Route("api/bau")]
    [ApiController]
    [Authorize(Roles = RoleNames.BauUser)]
    public class BauCacheController : ControllerBase
    {
        private readonly IBlobStorageService _privateBlobStorageService;
        private readonly IBlobStorageService _publicBlobStorageService;

        public BauCacheController(
            IBlobStorageService privateBlobStorageService,
            IBlobStorageService publicBlobStorageService)
        {
            _privateBlobStorageService = privateBlobStorageService;
            _publicBlobStorageService = publicBlobStorageService;
        }

        [HttpDelete("private-cache")]
        public async Task<ActionResult> ClearPrivateCache()
        {
            // Remove all cache entries as we currently don't really need
            // any more granularity currently (we only cache data blocks).
            await _privateBlobStorageService.DeleteBlobs(BlobContainers.PrivateContent);

            return NoContent();
        }

        [HttpDelete("public-cache/trees")]
        public async Task<ActionResult> ClearPublicCacheTrees(ClearCacheTreePathsViewModel request)
        {
            if (request.Paths.Any())
            {
                await request.Paths
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(
                        path =>
                            _publicBlobStorageService.DeleteBlob(BlobContainers.PublicContent, path)
                    );
            }

            return NoContent();
        }

        [HttpDelete("public-cache/releases")]
        public async Task<ActionResult> ClearPublicCacheReleases(ClearCacheReleasePathsViewModel request)
        {
            if (request.Paths.Any())
            {
                var pathString = request.Paths.JoinToString('|');

                await _publicBlobStorageService.DeleteBlobs(
                    BlobContainers.PublicContent,
                    options: new DeleteBlobsOptions
                    {
                        IncludeRegex = new Regex($"publications/.*/releases/.*/({pathString})/")
                    }
                );
            }

            return NoContent();
        }

        public class ClearCacheTreePathsViewModel
        {
            private static readonly HashSet<string> AllowedPaths = new HashSet<string>
            {
                PublicationTreeCacheKey.GetKey(),
                AllMethodologiesCacheKey.GetKey()
            };

            [MinLength(1)]
            [ContainsOnly(AllowedValuesProvider = nameof(AllowedPaths))]
            public HashSet<string> Paths { get; set; } = new HashSet<string>();
        }

        public class ClearCacheReleasePathsViewModel
        {
            private static readonly HashSet<string> AllowedPaths = new HashSet<string>
            {
                FileStoragePathUtils.PublicContentDataBlocksDirectory,
                // TODO: EES-2865 Remove with other fast track code
                FileStoragePathUtils.PublicContentFastTrackResultsDirectory,
                FileStoragePathUtils.PublicContentSubjectMetaDirectory
            };

            [MinLength(1)]
            [ContainsOnly(AllowedValuesProvider = nameof(AllowedPaths))]
            public HashSet<string> Paths { get; set; } = new HashSet<string>();
        }
    }
}
