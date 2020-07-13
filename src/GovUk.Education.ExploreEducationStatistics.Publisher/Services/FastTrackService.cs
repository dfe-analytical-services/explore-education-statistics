using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class FastTrackService : IFastTrackService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly ITableStorageService _tableStorageService;

        public FastTrackService(ContentDbContext contentDbContext,
            IFileStorageService fileStorageService,
            ITableStorageService tableStorageService)
        {
            _contentDbContext = contentDbContext;
            _fileStorageService = fileStorageService;
            _tableStorageService = tableStorageService;
        }

        public async Task CreateAllByRelease(Guid releaseId, PublishContext context)
        {
            // Delete any existing FastTracks in case of republishing
            await DeleteAllFastTracksByRelease(releaseId);

            var dataBlocks = await _contentDbContext.ReleaseContentBlocks
                .Include(block => block.ContentBlock)
                .Where(block => block.ReleaseId == releaseId)
                .Select(block => block.ContentBlock)
                .OfType<DataBlock>()
                .ToListAsync();

            foreach (var dataBlock in dataBlocks)
            {
                // Create one FastTrack per DataBlock
                var fastTrack = await CreateFastTrack(releaseId, dataBlock);
                await Upload(releaseId, fastTrack, context);
            }
        }

        private async Task<FastTrack> CreateFastTrack(Guid releaseId, DataBlock dataBlock)
        {
            var table = await GetTableAsync();
            // Use the DataBlock id as the FastTrack id
            var releaseFastTrack = new ReleaseFastTrack(releaseId, dataBlock.Id, dataBlock.HighlightName);
            await table.ExecuteAsync(TableOperation.Insert(releaseFastTrack));
            return new FastTrack(dataBlock.Id, dataBlock.Table, dataBlock.Query, releaseId);
        }

        public async Task DeleteAllReleaseFastTracks()
        {
            var allPartitionKeys = (await _tableStorageService.ExecuteQueryAsync(PublicReleaseFastTrackTableName,
                    new TableQuery<ReleaseFastTrack>()))
                .Select(track => track.PartitionKey)
                .Distinct();

            await _tableStorageService.DeleteByPartitionKeys(PublicReleaseFastTrackTableName, allPartitionKeys);
        }

        private async Task DeleteAllFastTracksByRelease(Guid releaseId)
        {
            await _fileStorageService.DeletePublicBlobs(PublicContentReleaseFastTrackPath(releaseId.ToString()));
            await _tableStorageService.DeleteByPartitionKey(PublicReleaseFastTrackTableName, releaseId.ToString());
        }

        private async Task Upload(Guid releaseId, FastTrack fastTrack, PublishContext context)
        {
            var blobName = context.Staging
                ? PublicContentFastTrackPath(releaseId.ToString(), fastTrack.Id.ToString(), PublicContentStagingPath())
                : PublicContentFastTrackPath(releaseId.ToString(), fastTrack.Id.ToString());
            await _fileStorageService.UploadAsJson(blobName, fastTrack);
        }

        private async Task<CloudTable> GetTableAsync()
        {
            return await _tableStorageService.GetTableAsync(PublicReleaseFastTrackTableName);
        }
    }
}