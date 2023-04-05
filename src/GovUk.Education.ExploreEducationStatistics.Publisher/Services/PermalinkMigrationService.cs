#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
public class PermalinkMigrationService : IPermalinkMigrationService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly BlobServiceClient _blobServiceClient;

    public PermalinkMigrationService(ContentDbContext contentDbContext,
        BlobServiceClient blobServiceClient)
    {
        _contentDbContext = contentDbContext;
        _blobServiceClient = blobServiceClient;
    }

    public async Task<Permalink> MigratePermalink(Guid permalinkId)
    {
        var permalink = await _contentDbContext.Permalinks
            .SingleOrDefaultAsync(p => p.Id == permalinkId);

        if (permalink == null)
        {
            throw new InvalidOperationException($"Permalink with id {permalinkId} not found");
        }

        var permalinkBlob = await GetPermalinkBlob(permalinkId);
        BlobProperties blobProperties = await permalinkBlob.GetPropertiesAsync();

        // EES-4201 Update permalink to store the blob content length and other statistics
        _contentDbContext.Permalinks.Update(permalink);

        var statistics = await GetStatistics(permalinkId, permalinkBlob);
        permalink.CountFilterItems = statistics.CountFilterItems;
        permalink.CountFootnotes = statistics.CountFootnotes;
        permalink.CountIndicators = statistics.CountIndicators;
        permalink.CountLocations = statistics.CountLocations;
        permalink.CountObservations = statistics.CountObservations;
        permalink.CountTimePeriods = statistics.CountTimePeriods;
        permalink.LegacyHasConfigurationHeaders = statistics.HasConfigurationHeaders;

        permalink.LegacyContentLength = blobProperties.ContentLength;

        await _contentDbContext.SaveChangesAsync();

        return permalink;
    }

    private async Task<BlobClient> GetPermalinkBlob(Guid permalinkId)
    {
        var blobContainer = _blobServiceClient.GetBlobContainerClient(BlobContainers.Permalinks.Name);
        var blob = blobContainer.GetBlobClient(permalinkId.ToString());

        if (!await blob.ExistsAsync())
        {
            throw new InvalidOperationException($"Blob not found for permalink {permalinkId}");
        }

        return blob;
    }

    private static async Task<PermalinkStatistics> GetStatistics(Guid permalinkId, BlobClient blob)
    {
        await using var stream = await blob.OpenReadAsync();
        using var streamReader = new StreamReader(stream);
        var jsonReader = new JsonTextReader(streamReader);

        var jsonObject = await JToken.ReadFromAsync(jsonReader);

        if (jsonObject.Value<string>("Id") != permalinkId.ToString())
        {
            throw new InvalidOperationException($"Blob does not match expected Permalink id {permalinkId}");
        }

        var configuration = jsonObject.SelectToken("Configuration");
        var fullTable = jsonObject.SelectToken("FullTable");
        var subjectMeta = fullTable?.SelectToken("SubjectMeta");

        var filterGroupsOptions = subjectMeta?.SelectTokens("$.Filters.*.Options.*.Options");

        var countFilterItems =
            filterGroupsOptions?.Sum(filterGroupOptions => (filterGroupOptions as JArray)?.Count) ?? 0;
        var countFootnotes = (subjectMeta?.SelectToken("Footnotes") as JArray)?.Count ?? 0;
        var countIndicators = (subjectMeta?.SelectToken("Indicators") as JArray)?.Count ?? 0;
        var countTimePeriods = (subjectMeta?.SelectToken("TimePeriodRange") as JArray)?.Count ?? 0;
        var countObservations = (fullTable?.SelectToken("Results") as JArray)?.Count ?? 0;
        var hasConfigurationHeaders = configuration?.SelectToken("TableHeaders")?.HasValues ?? false;

        // Presence of elements in 'LocationsHierarchical' means 'Locations' is ignored
        var locationsHierarchical = subjectMeta?.SelectToken("LocationsHierarchical");
        var countLocations = locationsHierarchical != null
            ? CountLocationsHierarchical(locationsHierarchical)
            : (subjectMeta?.SelectToken("Locations") as JArray)?.Count ?? 0;

        return new PermalinkStatistics(CountFilterItems: countFilterItems,
            CountFootnotes: countFootnotes,
            CountIndicators: countIndicators,
            CountLocations: countLocations,
            CountTimePeriods: countTimePeriods,
            CountObservations: countObservations,
            HasConfigurationHeaders: hasConfigurationHeaders);
    }

    private static int CountLocationsHierarchical(JToken locationsHierarchical)
    {
        return locationsHierarchical.Sum(level =>
            (level as JProperty)?.Value is JArray levelAttributes ? CountLocationAttributes(levelAttributes) : 0);
    }

    private static int CountLocationAttributes(JArray locationAttributes)
    {
        return locationAttributes.Sum(locationAttribute =>
        {
            if (locationAttribute["Options"] is not JArray options)
            {
                // If the location attribute has no "Options", it's a leaf node
                return 1;
            }

            return CountLocationAttributes(options);
        });
    }

    private record PermalinkStatistics(int CountFilterItems,
        int CountFootnotes,
        int CountIndicators,
        int CountLocations,
        int CountObservations,
        int CountTimePeriods,
        bool HasConfigurationHeaders);
}
