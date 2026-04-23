#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Common.Constants;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.IBlobStorageService;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;

[Route("api/bau")]
[ApiController]
[Authorize(Roles = RoleNames.BauUser)]
public class BauCacheController : ControllerBase
{
    private readonly IPrivateBlobStorageService _privateBlobStorageService;
    private readonly IPublicBlobStorageService _publicBlobStorageService;
    private readonly IGlossaryCacheService _glossaryCacheService;
    private readonly IMethodologyCacheService _methodologyCacheService;
    private readonly IPublicationsTreeService _publicationsTreeService;

    private const string ReleasePeriodPattern = "[0-9]{4}(-[^/]+)?\\";
    private const string BoundaryLevelIdPattern = "\\d{1,3}";

    public BauCacheController(
        IPrivateBlobStorageService privateBlobStorageService,
        IPublicBlobStorageService publicBlobStorageService,
        IGlossaryCacheService glossaryCacheService,
        IMethodologyCacheService methodologyCacheService,
        IPublicationsTreeService publicationsTreeService
    )
    {
        _privateBlobStorageService = privateBlobStorageService;
        _publicBlobStorageService = publicBlobStorageService;
        _glossaryCacheService = glossaryCacheService;
        _methodologyCacheService = methodologyCacheService;
        _publicationsTreeService = publicationsTreeService;
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
                    IncludeRegex = new Regex($"^releases/{RegexPatterns.WildcardDirectoryName}/({pathString})/"),
                }
            );
        }

        return NoContent();
    }

    [HttpPut("public-cache/glossary")]
    public async Task<ActionResult> UpdatePublicCacheGlossary()
    {
        await _glossaryCacheService.UpdateGlossary();
        return NoContent();
    }

    [HttpPut("public-cache/trees")]
    public async Task<ActionResult> UpdatePublicCacheTrees(UpdatePublicCacheTreePathsViewModel request)
    {
        if (request.CacheEntries.Any())
        {
            await request
                .CacheEntries.ToAsyncEnumerable()
                .ForEachAwaitAsync(async entry =>
                {
                    var allowedPath = EnumUtil.GetFromEnumValue<UpdatePublicCacheTreePathsViewModel.CacheEntry>(entry);

                    switch (allowedPath)
                    {
                        case UpdatePublicCacheTreePathsViewModel.CacheEntry.MethodologyTree:
                            await _methodologyCacheService.UpdateSummariesTree();
                            break;
                        case UpdatePublicCacheTreePathsViewModel.CacheEntry.PublicationTree:
                            await _publicationsTreeService.UpdateCachedPublicationsTree();
                            break;
                        default:
                            throw new ArgumentException($"Unsupported cache entry {entry}");
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
                    IncludeRegex = new Regex(
                        $"^publications/{RegexPatterns.WildcardDirectoryName}/releases/{RegexPatterns.WildcardDirectoryName}/({pathString})/"
                    ),
                }
            );
        }

        return NoContent();
    }

    [HttpDelete("public-cache/publication/{publicationSlug}")]
    public async Task<ActionResult> ClearPublicCachePublication(string publicationSlug)
    {
        await _publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            options: new DeleteBlobsOptions { IncludeRegex = new Regex($"^publications/{publicationSlug}/.+$") }
        );

        return NoContent();
    }

    [HttpDelete("public-cache/publications")]
    public async Task<ActionResult> ClearPublicCachePublications()
    {
        await _publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            options: new DeleteBlobsOptions { IncludeRegex = new Regex("^publications/.+$") }
        );

        return NoContent();
    }

    /// <summary>
    /// Clears all GeoJSON cached for Admin.
    ///
    /// Useful for when the GeoJSON cache needs to be refreshed, without impacting associated data blocks.
    /// </summary>
    [HttpDelete("private-cache/data-blocks/geojson")]
    public async Task<ActionResult> ClearPrivateGeoJsonCacheJson()
    {
        await _privateBlobStorageService.DeleteBlobs(
            BlobContainers.PrivateContent,
            options: new DeleteBlobsOptions
            {
                // Match against pattern: releases/***/data-blocks/GUID-boundary-levels/GUID-2.json
                IncludeRegex = new Regex(
                    $"^releases/{RegexPatterns.WildcardDirectoryName}/data-blocks/{RegexPatterns.Guid}-boundary-levels/{Constants.RegexPatterns.Guid}-{BoundaryLevelIdPattern}\\.json$"
                ),
            }
        );

        return NoContent();
    }

    /// <summary>
    /// Clears all GeoJSON cached for Data.
    ///
    /// Useful for when the GeoJSON cache needs to be refreshed, without impacting associated data blocks.
    /// </summary>
    [HttpDelete("public-cache/data-blocks/geojson")]
    public async Task<ActionResult> ClearPublicGeoJsonCacheJson()
    {
        await _publicBlobStorageService.DeleteBlobs(
            BlobContainers.PublicContent,
            options: new DeleteBlobsOptions
            {
                // Match against patterns:
                //
                // publications/***/releases/1234/data-blocks/GUID-boundary-levels/GUID-1.json
                // publications/***/releases/1234-35/data-blocks/GUID-boundary-levels/GUID-12.json
                // publications/***/releases/1234-q1/data-blocks/GUID-boundary-levels/GUID-12.json
                // publications/***/releases/1234-35-q1/data-blocks/GUID-boundary-levels/GUID-123.json
                IncludeRegex = new Regex(
                    $"^publications/{RegexPatterns.WildcardDirectoryName}/releases/{ReleasePeriodPattern}/data-blocks/{Constants.RegexPatterns.Guid}-boundary-levels/{Constants.RegexPatterns.Guid}-{BoundaryLevelIdPattern}\\.json$"
                ),
            }
        );

        return NoContent();
    }

    public class UpdatePublicCacheTreePathsViewModel
    {
        public enum CacheEntry
        {
            MethodologyTree,
            PublicationTree,
        }

        private static readonly HashSet<string> AllowedCacheEntries = new()
        {
            CacheEntry.MethodologyTree.ToString(),
            CacheEntry.PublicationTree.ToString(),
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
            FileStoragePathUtils.SubjectMetaDirectory,
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
            FileStoragePathUtils.SubjectMetaDirectory,
        };

        [MinLength(1)]
        [ContainsOnly(AllowedValuesProvider = nameof(AllowedPaths))]
        public HashSet<string> Paths { get; set; } = new();
    }
}
