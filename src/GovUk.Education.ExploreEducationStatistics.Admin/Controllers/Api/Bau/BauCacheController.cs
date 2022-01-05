#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
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
        private static readonly HashSet<string> AllowedPublicReleasePaths = new HashSet<string>
        {
            FileStoragePathUtils.PublicContentDataBlocksDirectory,
            // TODO: EES-2865 Remove with other fast track code
            FileStoragePathUtils.PublicContentFastTrackResultsDirectory,
            FileStoragePathUtils.PublicContentSubjectMetaDirectory
        };

        private static readonly HashSet<string> AllowedPublicTreePaths = new HashSet<string>
        {
            PublicationTreeCacheKey.GetKey(PublicationTreeFilter.AnyData),
            PublicationTreeCacheKey.GetKey(PublicationTreeFilter.LatestData),
            PublicationTreeCacheKey.GetKey(null)
        };

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
        public async Task<ActionResult> ClearPublicCacheTrees(ClearCachePathsViewModel request)
        {
            var paths = request.Paths
                .Where(AllowedPublicTreePaths.Contains)
                .ToHashSet();

            if (paths.Any())
            {
                await paths
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(
                        path =>
                            _publicBlobStorageService.DeleteBlob(BlobContainers.PublicContent, path)
                    );
            }

            return NoContent();
        }

        [HttpDelete("public-cache/releases")]
        public async Task<ActionResult> ClearPublicCacheReleases(ClearCachePathsViewModel request)
        {
            // Can only allow certain paths right now as not all cached items
            // are automatically regenerated (e.g. when a user first visits
            // the relevant part of the frontend).
            var paths = request.Paths
                .Where(AllowedPublicReleasePaths.Contains)
                .ToHashSet();

            if (paths.Any())
            {
                var pathString = paths.JoinToString('|');

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

        public class ClearCachePathsViewModel
        {
            [Required]
            public List<string> Paths { get; set; } = new List<string>();
        }
    }
}