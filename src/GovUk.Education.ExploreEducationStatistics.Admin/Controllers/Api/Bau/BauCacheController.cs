#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
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
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IThemeCacheService _themeCacheService;

        public BauCacheController(
            IBlobStorageService privateBlobStorageService,
            IBlobStorageService publicBlobStorageService, 
            IMethodologyCacheService methodologyCacheService, 
            IThemeCacheService themeCacheService)
        {
            _privateBlobStorageService = privateBlobStorageService;
            _publicBlobStorageService = publicBlobStorageService;
            _methodologyCacheService = methodologyCacheService;
            _themeCacheService = themeCacheService;
        }

        [HttpDelete("private-cache")]
        public async Task<ActionResult> ClearPrivateCache(ClearPrivateCachePathsViewModel request)
        {
            if (request.Paths.Any())
            {
                var pathString = request.Paths.JoinToString('|');

                await _privateBlobStorageService.DeleteBlobs(
                    BlobContainers.PrivateContent,
                    options: new DeleteBlobsOptions
                    {
                        IncludeRegex = new Regex($"^releases/.*/({pathString})/")
                    }
                );
            }

            return NoContent();
        }

        [HttpDelete("public-cache/trees")]
        public async Task<ActionResult> ClearPublicCacheTrees(ClearPublicCacheTreePathsViewModel request)
        {
            if (request.CacheEntries.Any())
            {
                await request.CacheEntries
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(
                        
                        async entry =>
                        {
                            var allowedPath =
                                EnumUtil.GetFromString<ClearPublicCacheTreePathsViewModel.CacheEntry>(entry);
                            
                            switch (allowedPath)
                            {
                                case ClearPublicCacheTreePathsViewModel.CacheEntry.MethodologyTree: 
                                    await _methodologyCacheService.UpdateSummariesTree();
                                    break;
                                case ClearPublicCacheTreePathsViewModel.CacheEntry.PublicationTree:
                                    await _themeCacheService.UpdatePublicationTree();
                                    break;
                                default:
                                    throw new ArgumentException($"Unsupported cache clearing entry {entry}");
                            }
                        });
            }

            return NoContent();
        }

        [HttpDelete("public-cache/releases")]
        public async Task<ActionResult> ClearPublicCacheReleases(ClearPublicCacheReleasePathsViewModel request)
        {
            if (request.Paths.Any())
            {
                var pathString = request.Paths.JoinToString('|');

                await _publicBlobStorageService.DeleteBlobs(
                    BlobContainers.PublicContent,
                    options: new DeleteBlobsOptions
                    {
                        IncludeRegex = new Regex($"^publications/.*/releases/.*/({pathString})/")
                    }
                );
            }

            return NoContent();
        }

        public class ClearPublicCacheTreePathsViewModel
        {
            public enum CacheEntry
            {
                MethodologyTree,
                PublicationTree
            }
            
            private static readonly HashSet<string> AllowedCacheEntries = new()
            {
                CacheEntry.MethodologyTree.ToString(),
                CacheEntry.PublicationTree.ToString()
            };

            [MinLength(1)]
            [ContainsOnly(AllowedValuesProvider = nameof(AllowedCacheEntries))]
            public HashSet<string> CacheEntries { get; set; } = new();
        }

        public class ClearPublicCacheReleasePathsViewModel
        {
            private static readonly HashSet<string> AllowedPaths = new()
            {
                FileStoragePathUtils.DataBlocksDirectory,
                FileStoragePathUtils.SubjectMetaDirectory
            };

            [MinLength(1)]
            [ContainsOnly(AllowedValuesProvider = nameof(AllowedPaths))]
            public HashSet<string> Paths { get; set; } = new();
        }

        public class ClearPrivateCachePathsViewModel
        {
            private static readonly HashSet<string> AllowedPaths = new()
            {
                FileStoragePathUtils.DataBlocksDirectory,
                FileStoragePathUtils.SubjectMetaDirectory
            };

            [MinLength(1)]
            [ContainsOnly(AllowedValuesProvider = nameof(AllowedPaths))]
            public HashSet<string> Paths { get; set; } = new();
        }
    }
}
