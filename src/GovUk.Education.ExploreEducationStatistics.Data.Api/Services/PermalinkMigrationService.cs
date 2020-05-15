using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public class PermalinkMigrationService : IPermalinkMigrationService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger _logger;

        private const string PermalinkContainerName = PermalinkService.ContainerName;
        private const string MigrationContainerName = "permalink-migrations";

        public PermalinkMigrationService(IFileStorageService fileStorageService,
            ILogger<PermalinkMigrationService> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<bool> MigrateAll<T>(string migrationId,
            Func<T, Task<Either<string, Permalink>>> transformFunc)
        {
            var migrationHistoryWriter =
                await MigrationHistoryWriter.CreateAsync(MigrationContainerName, migrationId, _fileStorageService);
            var shouldRun = await CheckMigrationShouldRunAndRecordHistoryAsync(migrationHistoryWriter);
            if (!shouldRun)
            {
                _logger.LogInformation("Skipping Permalink migration: {MigrationId}", migrationId);
                return false;
            }

            var source = await DownloadPermalinksAsync<T>();
            var (errors, transformed) = await DoTransform(source, transformFunc);

            if (errors.Any())
            {
                _logger.LogError("Permalink migration: {MigrationId} finished with errors", migrationId);
                await migrationHistoryWriter.WriteHistoryAsync(string.Join(Environment.NewLine, errors));
                return false;
            }

            await UploadPermalinksAsync(migrationHistoryWriter, transformed);
            _logger.LogInformation("Permalink migration: {MigrationId} finished successfully", migrationId);
            return true;
        }

        private async Task<(List<string> errors, List<Permalink> transformed)> DoTransform<T>(
            List<T> source,
            Func<T, Task<Either<string, Permalink>>> transformFunc)
        {
            var count = source.Count;
            var errors = new List<string>();
            var transformed = new List<Permalink>();
            foreach (var (permalink, index) in source.WithIndex())
            {
                try
                {
                    _logger.LogDebug("Transforming {Type} {Index} of {Count}", typeof(T).Name, index + 1, count);
                    var result = await transformFunc.Invoke(permalink);
                    if (result.IsLeft)
                    {
                        var error = result.Left;
                        _logger.LogError("Error while transforming {Index} of {Count}: {Error}", index + 1, count, error);
                        errors.Add(result.Left);
                    }
                    else
                    {
                        transformed.Add(result.Right);
                    }
                }
                catch (Exception e)
                {
                    // TODO Not ideal because this doesn't identify the Permalink in the error
                    _logger.LogError(e, "Exception occured while transforming Permalink");
                    errors.Add($"Exception occured while transforming Permalink: {e.GetType()} {e.Message}");
                }
            }

            return (errors, transformed);
        }

        private async Task<List<T>> DownloadPermalinksAsync<T>()
        {
            _logger.LogDebug("Listing blobs in container: {Container}", PermalinkContainerName);
            var blobs = _fileStorageService.ListBlobs(PermalinkContainerName).ToList();
            _logger.LogDebug("Found {Count} blobs in container: {Container}", blobs.Count, PermalinkContainerName);
            var strings = await Task.WhenAll(blobs.Select(blob => blob.DownloadTextAsync()));
            _logger.LogDebug("Downloaded {Count} blobs", strings.Length);
            var permalinks = strings.Select(JsonConvert.DeserializeObject<T>).ToList();
            _logger.LogDebug("Deserialized {Count} blobs as type {Type}", permalinks.Count, typeof(T).Name);
            return permalinks;
        }

        private async Task UploadPermalinksAsync(MigrationHistoryWriter migrationHistoryWriter,
            List<Permalink> permalinks)
        {
            _logger.LogDebug("Uploading {Count} Permalinks", permalinks.Count);
            await migrationHistoryWriter.WriteHistoryAsync($"Uploading {permalinks.Count} Permalinks");
            await Task.WhenAll(permalinks.Select(permalink =>
                _fileStorageService.UploadFromStreamAsync(PermalinkContainerName,
                    permalink.Id.ToString(),
                    MediaTypeNames.Application.Json,
                    JsonConvert.SerializeObject(permalink))));
            _logger.LogDebug("Upload complete");
            await migrationHistoryWriter.WriteHistoryAsync("Upload complete");
        }

        private static async Task<bool> CheckMigrationShouldRunAndRecordHistoryAsync(
            MigrationHistoryWriter migrationHistoryWriter)
        {
            var shouldRun = !migrationHistoryWriter.IsHistoryExists();
            // Presence of history file is used to prevent future executions
            if (shouldRun)
            {
                await migrationHistoryWriter.WriteHistoryAsync("Started");
            }

            return shouldRun;
        }
    }

    internal class MigrationHistoryWriter
    {
        private readonly string _containerName;
        private readonly string _migrationId;
        private readonly IFileStorageService _fileStorageService;
        private readonly bool _appendSupported;

        private MigrationHistoryWriter(string containerName, string migrationId, IFileStorageService fileStorageService,
            bool appendSupported)
        {
            _containerName = containerName;
            _migrationId = migrationId;
            _fileStorageService = fileStorageService;
            _appendSupported = appendSupported;
        }

        internal static async Task<MigrationHistoryWriter> CreateAsync(string containerName, string migrationId,
            IFileStorageService fileStorageService)
        {
            var appendSupported = await fileStorageService.TryGetOrCreateAppendBlobAsync(containerName, migrationId);
            return new MigrationHistoryWriter(containerName, migrationId, fileStorageService, appendSupported);
        }

        public bool IsHistoryExists()
        {
            var blob = _fileStorageService.GetBlob(_containerName, _migrationId);
            if (!blob.Exists())
            {
                return false;
            }

            blob.FetchAttributes();
            return blob.Properties.Length > 0;
        }

        public Task WriteHistoryAsync(string message)
        {
            if (_appendSupported)
            {
                return AppendMigrationHistoryAsync(message);
            }

            // Appending is not supported by the Storage Emulator.
            // Currently history is lost as last log message is replaced instead
            return UploadMigrationHistoryAsync(message);
        }

        private Task UploadMigrationHistoryAsync(string message)
        {
            var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            return _fileStorageService.UploadFromStreamAsync(_containerName,
                _migrationId,
                MediaTypeNames.Text.Plain,
                $"{now}: {message}");
        }

        private Task AppendMigrationHistoryAsync(string message)
        {
            var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            return _fileStorageService.AppendFromStreamAsync(_containerName,
                _migrationId,
                MediaTypeNames.Text.Plain,
                $"{now}: {message}{Environment.NewLine}");
        }
    }
}