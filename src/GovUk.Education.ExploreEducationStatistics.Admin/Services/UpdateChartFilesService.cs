using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UpdateChartFilesService : IUpdateChartFilesService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly ILogger _logger;

        private readonly string _storageConnectionString;
        
        public UpdateChartFilesService(
            IConfiguration configuration,
            ContentDbContext contentDbContext,
            ILogger<UpdateChartFilesService> logger)
        {
            _storageConnectionString = configuration.GetValue<string>("CoreStorage");
            _contentDbContext = contentDbContext;
            _logger = logger;
        }

        public async Task<Either<ActionResult, bool>> UpdateChartFiles()
        {
            var refs = _contentDbContext
                .ReleaseFileReferences
                .Where(rfr => rfr.ReleaseFileType == ReleaseFileTypes.Chart)
                .ToList();

            var result = await UpdateChartFilesAsync(refs);

            if (result.IsLeft)
            {
                return false;
            }

            return true;
        }

        private async Task<Either<ActionResult, bool>> UpdateChartFilesAsync(List<ReleaseFileReference> chartReferences)
        {
            var blobContainer = await GetCloudBlobContainer();

            foreach (var fileReference in chartReferences)
            {
                // First check if this reference has been converted - if not then rename in storage
                if (!Guid.TryParse(fileReference.Filename, out var guid))
                {
                    var oldName = fileReference.Filename;
                    var oldBlobPath = AdminReleasePathWithFilename(fileReference.ReleaseId, oldName);

                    // Files should exists in storage but if not then allow user to delete
                    if (blobContainer.GetBlockBlobReference(oldBlobPath).Exists())
                    {
                        var newId = Guid.NewGuid();

                        if (await CopyBlob(fileReference, blobContainer, oldBlobPath, newId))
                        {
                            // Change ref to any datablocks with charts where used

                            await UpdateDataBlock(fileReference, newId);

                            // Change the file reference name
                            fileReference.Filename = newId.ToString();
                            _contentDbContext.ReleaseFileReferences.Update(fileReference);

                            await _contentDbContext.SaveChangesAsync();

                            _logger.LogInformation(
                                $"Update blob: {fileReference.ReleaseId}:{fileReference.Filename} successfully");
                        }
                    }
                }
            }

            return true;
        }

        private async Task<bool> CopyBlob(ReleaseFileReference fileReference, CloudBlobContainer blobContainer, string oldBlobPath, Guid newId)
        {
            var newBlobPath = AdminReleasePathWithFilename(fileReference.ReleaseId, newId.ToString());

            var sourceBlob = blobContainer.GetBlockBlobReference(oldBlobPath);
            var targetBlob = blobContainer.GetBlockBlobReference(newBlobPath);
                        
            await targetBlob.StartCopyAsync(sourceBlob);

            while (targetBlob.CopyState.Status == CopyStatus.Pending) {
                await Task.Delay(1000);
                targetBlob.FetchAttributes();
            }

            if (targetBlob.CopyState.Status != CopyStatus.Success) {
                _logger.LogError($"Failed to copy blob: {fileReference.ReleaseId}:{fileReference.Filename}");
                return false;
            }

            await sourceBlob.DeleteAsync();
            
            _logger.LogInformation($"Copied blob: {fileReference.ReleaseId}:{fileReference.Filename} successfully");

            return true;
        }
        
        private async Task UpdateDataBlock(ReleaseFileReference releaseFileReference, Guid newChartId)
        {
            var releaseIds = await _contentDbContext.ReleaseFiles
                .Where(rf => rf.ReleaseFileReferenceId == releaseFileReference.Id)
                .Select(rf => rf.ReleaseId)
                .Distinct()
                .ToListAsync();
            
            var blocks = GetDataBlocks(releaseIds);

            foreach (var block in blocks)
            {
                var infoGraphicChart = block.Charts
                    .OfType<InfographicChart>()
                    .FirstOrDefault();

                if (infoGraphicChart != null && infoGraphicChart.FileId == releaseFileReference.Filename)
                {
                    block.Charts.Remove(infoGraphicChart);
                    infoGraphicChart.FileId = newChartId.ToString();
                    block.Charts.Add(infoGraphicChart);
                    _contentDbContext.DataBlocks.Update(block);
                }
            }
        }
        
        private List<DataBlock> GetDataBlocks(List<Guid> releaseIds)
        {
            return _contentDbContext
                .ReleaseContentBlocks
                .Include(join => join.ContentBlock)
                .ThenInclude(block => block.ContentSection)
                .Where(join => releaseIds.Contains(join.ReleaseId))
                .ToList()
                .Select(join => join.ContentBlock)
                .OfType<DataBlock>()
                .ToList();
        }
        
        private async Task<CloudBlobContainer> GetCloudBlobContainer()
        {
            return await GetCloudBlobContainerAsync(_storageConnectionString, PrivateFilesContainerName);
        }
        
        private static async Task<CloudBlobContainer> GetCloudBlobContainerAsync(string storageConnectionString,
            string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            return blobContainer;
        }
        
        private string AdminReleasePathWithFilename(Guid releaseId, string fileName)
        {
            return AdminReleasePath(releaseId, ReleaseFileTypes.Chart, fileName);
        }
    }
}