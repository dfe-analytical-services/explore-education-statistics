#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
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
    private readonly IPublicationCacheService _publicationCacheService;

    private const string GuidPattern = "[\\da-zA-Z]{8}-([\\da-zA-Z]{4}-){3}[\\da-zA-Z]{12}";
    private const string ReleasePeriodPattern = "[0-9]{4}(-[^/]+)?\\";
    private const string BoundaryLevelIdPattern = "\\d{1,3}";
    private const string WildcardDirectoryNamePattern = "[^/]+";

    public BauCacheController(
        IPrivateBlobStorageService privateBlobStorageService,
        IPublicBlobStorageService publicBlobStorageService,
        IGlossaryCacheService glossaryCacheService,
        IMethodologyCacheService methodologyCacheService,
        IPublicationCacheService publicationCacheService)
    {
        _privateBlobStorageService = privateBlobStorageService;
        _publicBlobStorageService = publicBlobStorageService;
        _glossaryCacheService = glossaryCacheService;
        _methodologyCacheService = methodologyCacheService;
        _publicationCacheService = publicationCacheService;
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
                    IncludeRegex = new Regex($"^releases/{WildcardDirectoryNamePattern}/({pathString})/")
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
            await request.CacheEntries
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(

                    async entry =>
                    {
                        var allowedPath =
                            EnumUtil.GetFromEnumValue<UpdatePublicCacheTreePathsViewModel.CacheEntry>(entry);

                        switch (allowedPath)
                        {
                            case UpdatePublicCacheTreePathsViewModel.CacheEntry.MethodologyTree:
                                await _methodologyCacheService.UpdateSummariesTree();
                                break;
                            case UpdatePublicCacheTreePathsViewModel.CacheEntry.PublicationTree:
                                await _publicationCacheService.UpdatePublicationTree();
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
                    IncludeRegex = new Regex($"^publications/{WildcardDirectoryNamePattern}/releases/{WildcardDirectoryNamePattern}/({pathString})/")
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
            options: new DeleteBlobsOptions
            {
                IncludeRegex = new Regex($"^publications/{publicationSlug}/.+$")
            }
        );

        return NoContent();
    }

    [HttpDelete("public-cache/publications")]
    public async Task<ActionResult> ClearPublicCachePublications()
    {
        await _publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            options: new DeleteBlobsOptions
            {
                IncludeRegex = new Regex("^publications/.+$")
            }
        );

        return NoContent();
    }

    /// <summary>
    /// Clears all publication.json files
    ///
    /// Useful for when the PublicationCacheViewModel has been changed to invalidate all publication JSON files.
    /// </summary>
    /// <returns></returns>
    [HttpDelete("public-cache/publications/publication-json")]
    public async Task<ActionResult> ClearPublicCachePublicationJson()
    {
        var publicationJsonFilenameRegex = FileStoragePathUtils.PublicationFileName.Replace(".", "\\.");

        await _publicBlobStorageService.DeleteBlobs(
            BlobContainers.PublicContent,
            options: new DeleteBlobsOptions
            {
                IncludeRegex = new Regex($"^publications/{WildcardDirectoryNamePattern}/{publicationJsonFilenameRegex}$")
            });

        return NoContent();
    }

    /// <summary>
    /// Clears all latest-release.json and releases/[yyyy-*].json.
    ///
    /// Useful for when the ReleaseCacheViewModel has been changed to invalidate all release JSON files, as this
    /// will target both the latest-release.json and individual [release-year-and-time-identifier].json files in
    /// the same call.
    /// </summary>
    [HttpDelete("public-cache/publications/release-json")]
    public async Task<ActionResult> ClearPublicCacheReleaseJson()
    {
        var latestReleaseJsonFilenameRegex = FileStoragePathUtils.LatestReleaseFileName.Replace(".", "\\.");

        await _publicBlobStorageService.DeleteBlobs(
            BlobContainers.PublicContent,
            options: new DeleteBlobsOptions
            {
                // Match against patterns:
                //
                // publications/***/latest-release.json
                // publications/***/releases/1234.json
                // publications/***/releases/1234-35.json
                // publications/***/releases/1234-q1.json
                // publications/***/releases/1234-35-q1.json
                IncludeRegex = new Regex($"^publications/{WildcardDirectoryNamePattern}/({latestReleaseJsonFilenameRegex}|releases/[0-9]{{4}}(-[^/]+)?\\.json)$")
            });

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
                IncludeRegex = new Regex($"^releases/{WildcardDirectoryNamePattern}/data-blocks/{GuidPattern}-boundary-levels/{GuidPattern}-{BoundaryLevelIdPattern}\\.json$")
            });

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
                IncludeRegex = new Regex($"^publications/{WildcardDirectoryNamePattern}/releases/{ReleasePeriodPattern}/data-blocks/{GuidPattern}-boundary-levels/{GuidPattern}-{BoundaryLevelIdPattern}\\.json$")
            });

        return NoContent();
    }

    public class UpdatePublicCacheTreePathsViewModel
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
