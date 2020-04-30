using System;
using System.Collections.Generic;
using System.Linq;
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

        public PermalinkMigrationService(IFileStorageService fileStorageService,
            ILogger<PermalinkMigrationService> logger)
        {
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<Either<ActionResult, bool>> MigrateAll<T>(
            Func<T, Task<Either<string, Permalink>>> transformFunc)
        {
            var blobs = _fileStorageService.ListBlobs(PermalinkService.ContainerName);
            var task = await Task.WhenAll(blobs.Select(blob => blob.DownloadTextAsync()));
            var permalinks = task.Select(JsonConvert.DeserializeObject<T>);

            var transformed = new List<Permalink>();
            var errors = new List<string>();

            foreach (var permalink in permalinks)
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

            transformed.ForEach(async permalink =>
            {
                await _fileStorageService.UploadFromStreamAsync(PermalinkService.ContainerName,
                    permalink.Id.ToString(),
                    "application/json", JsonConvert.SerializeObject(permalink));
            });
            return true;
        }
    }
}