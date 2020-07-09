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
            // Create one FastTrack per DataBlock using the DataBlock id as the FastTrack id
            var fastTracks = await _contentDbContext.ReleaseContentBlocks
                .Include(block => block.ContentBlock)
                .Where(block => block.ReleaseId == releaseId)
                .Select(block => block.ContentBlock)
                .OfType<DataBlock>()
                .Select(block => new FastTrack(block.Id, block.Table, block.Query, releaseId))
                .ToListAsync();

            foreach (var fastTrack in fastTracks)
            {
                await CreateReleaseFastTrack(releaseId, fastTrack);
                await Upload(releaseId, fastTrack, context);
            }
        }

        private async Task CreateReleaseFastTrack(Guid releaseId, FastTrack fastTrack)
        {
            var table = await GetTableAsync();
            var releaseFastTrack = new ReleaseFastTrack(releaseId, fastTrack.Id);
            await table.ExecuteAsync(TableOperation.Insert(releaseFastTrack));
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