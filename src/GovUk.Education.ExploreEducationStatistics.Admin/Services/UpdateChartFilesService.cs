using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
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
        private readonly  IUserService _userService;
        private readonly ILogger _logger;

        private readonly string _storageConnectionString;
        
        public UpdateChartFilesService(
            IConfiguration configuration,
            ContentDbContext contentDbContext,
            IUserService userService,
            ILogger<UpdateChartFilesService> logger)
        {
            _userService = userService;
            _storageConnectionString = configuration.GetValue<string>("CoreStorage");
            _contentDbContext = contentDbContext;
            _logger = logger;
        }

        public async Task<Either<ActionResult, Unit>> UpdateChartFiles()
        {
            var chartReferences = await _contentDbContext
                .ReleaseFileReferences
                .AsNoTracking()
                .Include(rfr => rfr.Release)
                .Where(rfr => rfr.ReleaseFileType == ReleaseFileTypes.Chart)
                .ToListAsync();

            var blobContainer = await GetCloudBlobContainer();

            foreach (var fileReference in chartReferences)
            {
                await _userService.CheckCanRunReleaseMigrations(fileReference.Release).OnSuccessDo(async _ =>
                {
                    var oldName = fileReference.Filename;
                    var oldBlobPath = AdminReleasePathWithFilename(fileReference.ReleaseId, oldName);

                    // Files should exists first time this is run in storage
                    if (blobContainer.GetBlockBlobReference(oldBlobPath).Exists())
                    {
                        if (await CopyBlob(fileReference, blobContainer, oldBlobPath, fileReference.Id))
                        {
                            // Change ref to any datablocks with charts where used

                            var releaseIds = await _contentDbContext.ReleaseFiles
                                .AsNoTracking()
                                .Where(rf => rf.ReleaseFileReferenceId == fileReference.Id)
                                .Select(rf => rf.ReleaseId)
                                .Distinct()
                                .ToListAsync();
                            
                            await UpdateDataBlocks(releaseIds, fileReference.Filename, fileReference.Id);

                            await _contentDbContext.SaveChangesAsync();

                            _logger.LogInformation(
                                $"Update blob: {fileReference.ReleaseId}:{fileReference.Filename} successfully");
                        }
                    }
                });
            }

            return Unit.Instance;
        }

        private async Task<bool> CopyBlob(ReleaseFileReference fileReference, CloudBlobContainer blobContainer, string oldBlobPath, Guid newId)
        {
            var newBlobPath = AdminReleasePathWithFilename(fileReference.ReleaseId, newId.ToString());

            var sourceBlob = blobContainer.GetBlockBlobReference(oldBlobPath);
            var targetBlob = blobContainer.GetBlockBlobReference(newBlobPath);
                        
            await targetBlob.StartCopyAsync(sourceBlob);
            targetBlob.FetchAttributes();
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
        
        private async Task UpdateDataBlocks(List<Guid> releaseIds, string oldName, Guid id)
        {
            var blocks = GetDataBlocks(releaseIds);
            
            foreach (var block in blocks)
            {
                var infoGraphicChart = block.Charts
                    .OfType<InfographicChart>()
                    .FirstOrDefault();

                if (infoGraphicChart != null && infoGraphicChart.FileId == oldName)
                {
                    block.Charts.Remove(infoGraphicChart);
                    infoGraphicChart.FileId = id.ToString();
                    block.Charts.Add(infoGraphicChart);
                    _contentDbContext.ContentBlocks.Update(block);
                }
            }
        }
        
        private List<DataBlock> GetDataBlocks(List<Guid> releaseIds)
        {
            return _contentDbContext
                .ReleaseContentBlocks
                .Include(rcb => rcb.ContentBlock)
                .ThenInclude(block => block.ContentSection)
                .Where(rcb => releaseIds.Contains(rcb.ReleaseId))
                .ToList()
                .Select(rcb => rcb.ContentBlock)
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