using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
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

        public async Task<Either<ActionResult, bool>> MigrateAll<T>(string migrationId,
            Func<T, Task<Either<string, Permalink>>> transformFunc)
        {
            var shouldRun = await CheckMigrationShouldRunAndRecordHistoryAsync(migrationId);

            if (shouldRun)
            {
                var source = await DownloadPermalinksAsync<T>();

                var transformed = new List<Permalink>();
                var errors = new List<string>();

                foreach (var permalink in source)
                {
                    var result = await transformFunc.Invoke(permalink);
                    if (result.IsLeft)
                    {
                        errors.Add(result.Left);
                    }
                    else
                    {
                        transformed.Add(result.Right);
                    }
                }

                if (errors.Any())
                {
                    errors.ForEach(message => _logger.LogError(message));
                    return false;
                }

                await UploadPermalinksAsync(transformed);

                return true;
            }

            return false;
        }

        private async Task<IEnumerable<T>> DownloadPermalinksAsync<T>()
        {
            var blobs = _fileStorageService.ListBlobs(PermalinkContainerName);
            var task = await Task.WhenAll(blobs.Select(blob => blob.DownloadTextAsync()));
            return task.Select(JsonConvert.DeserializeObject<T>);
        }

        private async Task UploadPermalinksAsync(List<Permalink> permalinks)
        {
            await Task.WhenAll(permalinks.Select(permalink =>
                _fileStorageService.UploadFromStreamAsync(PermalinkContainerName,
                    permalink.Id.ToString(),
                    MediaTypeNames.Text.Plain,
                    JsonConvert.SerializeObject(permalink))));
        }

        private async Task<bool> CheckMigrationShouldRunAndRecordHistoryAsync(string migrationId)
        {
            var shouldRun = !_fileStorageService.FileExists(MigrationContainerName, migrationId);
            if (shouldRun)
            {
                await AddMigrationHistoryAsync(migrationId);
            }

            return shouldRun;
        }

        private Task AddMigrationHistoryAsync(string migrationId)
        {
            var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            return _fileStorageService.UploadFromStreamAsync(MigrationContainerName,
                migrationId,
                MediaTypeNames.Text.Plain,
                now);
        }
    }
}