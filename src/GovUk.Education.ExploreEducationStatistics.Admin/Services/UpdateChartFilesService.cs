using System;
using System.Collections.Generic;
using System.Linq;
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
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class UpdateChartFilesService : IUpdateChartFilesService
    {
        private readonly ContentDbContext _contentDbContext;

        private readonly string _storageConnectionString;
        
        public UpdateChartFilesService(
            IConfiguration configuration,
            ContentDbContext contentDbContext)
        {
            _storageConnectionString = configuration.GetValue<string>("CoreStorage");
            _contentDbContext = contentDbContext;
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
                        var newBlobPath = AdminReleasePathWithFilename(fileReference.ReleaseId, newId.ToString());

                        var oldFile = blobContainer.GetBlockBlobReference(oldBlobPath);
                        var newFile = blobContainer.GetBlockBlobReference(newBlobPath);
                        
                        // Rename the blob
                        await newFile.StartCopyAsync(oldFile);  
                        await oldFile.DeleteAsync();
                        
                        // Change ref to any datablocks with charts where used

                        var releaseIds = _contentDbContext.ReleaseFiles
                            .Include(rf => rf.ReleaseFileReference)
                            .Where(rf => rf.ReleaseFileReference.Filename == oldName && rf.ReleaseFileReference.ReleaseFileType == ReleaseFileTypes.Chart)
                            .Select(rf => rf.ReleaseId)
                            .Distinct()
                            .ToList();

                        await UpdateDataBlock(releaseIds, oldName, newId);

                        // Change the file reference name
                        fileReference.Filename = newId.ToString();
                        _contentDbContext.ReleaseFileReferences.Update(fileReference);

                        await _contentDbContext.SaveChangesAsync();
                    }
                }
            }

            return true;
        }
        
        private async Task UpdateDataBlock(List<Guid> releaseIds, string oldChartName, Guid newChartId)
        {
            var blocks = GetDataBlocks(releaseIds);

            foreach (var block in blocks)
            {
                var infoGraphicChart = block.Charts
                    .OfType<InfographicChart>()
                    .FirstOrDefault();

                if (infoGraphicChart != null && infoGraphicChart.FileId == oldChartName)
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