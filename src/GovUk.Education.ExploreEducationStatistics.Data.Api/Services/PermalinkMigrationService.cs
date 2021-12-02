using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public class PermalinkMigrationService : IPermalinkMigrationService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger _logger;

        public PermalinkMigrationService(IBlobStorageService blobStorageService,
            ILogger<PermalinkMigrationService> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task<bool> MigrateAll<T>(string migrationId,
            Func<T, Task<Either<string, Permalink>>> transformFunc)
        {
            var migrationHistoryWriter =
                await MigrationHistoryWriter.Create(PermalinkMigrations, migrationId, _blobStorageService);
            var shouldRun = await CheckMigrationShouldRunAndRecordHistory(migrationHistoryWriter);
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
                await migrationHistoryWriter.WriteHistory(string.Join(Environment.NewLine, errors));
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
            _logger.LogDebug("Listing blobs in container: {Container}", Permalinks);

            var blobs = await _blobStorageService.ListBlobs(Permalinks);

            _logger.LogDebug("Found {Count} blobs in container: {Container}", blobs.Count, Permalinks);

            var strings = await Task.WhenAll(
                blobs.Select(blob =>
                    _blobStorageService.DownloadBlobText(
                        containerName: Permalinks,
                        path: blob.Path
                    )
                )
            );

            _logger.LogDebug("Downloaded {Count} blobs", strings.Length);

            var deserialized = new List<T>();

            foreach (var s in strings)
            {
                try
                {
                    deserialized.Add(JsonConvert.DeserializeObject<T>(s));
                }
                catch (JsonSerializationException e)
                {
                    _logger.LogError(e, "Caught Exception deserializing: {Blob}", s);
                }
            }
            _logger.LogDebug("Deserialized {Count} blobs as type {Type}", deserialized.Count, typeof(T).Name);
            return deserialized;
        }

        private async Task UploadPermalinksAsync(MigrationHistoryWriter migrationHistoryWriter,
            List<Permalink> permalinks)
        {
            _logger.LogDebug("Uploading {Count} Permalinks", permalinks.Count);
            await migrationHistoryWriter.WriteHistory($"Uploading {permalinks.Count} Permalinks");
            await Task.WhenAll(permalinks.Select(permalink =>
                _blobStorageService.UploadText(containerName: Permalinks,
                    path: permalink.Id.ToString(),
                    content: JsonConvert.SerializeObject(permalink),
                    contentType: MediaTypeNames.Application.Json)));
            _logger.LogDebug("Upload complete");
            await migrationHistoryWriter.WriteHistory("Upload complete");
        }

        private static async Task<bool> CheckMigrationShouldRunAndRecordHistory(
            MigrationHistoryWriter migrationHistoryWriter)
        {
            var shouldRun = !(await migrationHistoryWriter.IsHistoryExists());
            // Presence of history file is used to prevent future executions
            if (shouldRun)
            {
                await migrationHistoryWriter.WriteHistory("Started");
            }

            return shouldRun;
        }
    }

    internal class MigrationHistoryWriter
    {
        private readonly IBlobContainer _containerName;
        private readonly string _migrationId;
        private readonly IBlobStorageService _blobStorageService;
        private readonly bool _appendSupported;

        private MigrationHistoryWriter(IBlobContainer containerName,
            string migrationId,
            IBlobStorageService blobStorageService,
            bool appendSupported)
        {
            _containerName = containerName;
            _migrationId = migrationId;
            _blobStorageService = blobStorageService;
            _appendSupported = appendSupported;
        }

        internal static async Task<MigrationHistoryWriter> Create(IBlobContainer containerName, string migrationId,
            IBlobStorageService blobStorageService)
        {
            var appendSupported = await blobStorageService.IsAppendSupported(containerName, migrationId);
            return new MigrationHistoryWriter(containerName, migrationId, blobStorageService, appendSupported);
        }

        public async Task<bool> IsHistoryExists()
        {
            var exists = await _blobStorageService.CheckBlobExists(_containerName, _migrationId);

            if (!exists)
            {
                return false;
            }

            var blob = await _blobStorageService.GetBlob(_containerName, _migrationId);

            return blob.ContentLength > 0;
        }

        public Task WriteHistory(string message)
        {
            if (_appendSupported)
            {
                return AppendMigrationHistory(message);
            }

            // Appending is not supported by the Storage Emulator.
            // Currently history is lost as last log message is replaced instead
            return UploadMigrationHistory(message);
        }

        private Task UploadMigrationHistory(string message)
        {
            var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            return _blobStorageService.UploadText(containerName: _containerName,
                path: _migrationId,
                content: $"{now}: {message}",
                contentType: MediaTypeNames.Text.Plain);
        }

        private Task AppendMigrationHistory(string message)
        {
            var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            return _blobStorageService.AppendText(
                containerName: _containerName,
                path: _migrationId,
                content: $"{now}: {message}{Environment.NewLine}");
        }
    }
}
