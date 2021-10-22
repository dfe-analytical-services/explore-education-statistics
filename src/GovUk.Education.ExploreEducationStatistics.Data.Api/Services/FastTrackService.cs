using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Database.TimePeriodLabelFormat;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using StorageException = Microsoft.Azure.Storage.StorageException;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FastTrackService : IFastTrackService
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ITableStorageService _tableStorageService;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IMapper _mapper;

        public FastTrackService(
            ITableBuilderService tableBuilderService,
            IBlobStorageService blobStorageService,
            ITableStorageService tableStorageService,
            IReleaseRepository releaseRepository,
            IMapper mapper)
        {
            _tableBuilderService = tableBuilderService;
            _blobStorageService = blobStorageService;
            _tableStorageService = tableStorageService;
            _releaseRepository = releaseRepository;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, FastTrackViewModel>> GetFastTrackAndResults(Guid fastTrackId)
        {
            try
            {
                return await 
                    GetFastTrack(fastTrackId)
                    .OnSuccess(BuildViewModel);
            }
            catch (StorageException e)
                when ((HttpStatusCode) e.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        private Task<Either<ActionResult, FastTrackViewModel>> BuildViewModel(FastTrack fastTrack)
        {
            return _releaseRepository
                .FindOrNotFoundAsync(fastTrack.ReleaseId)
                .OnSuccessCombineWith(_ => _tableBuilderService.Query(fastTrack.ReleaseId, fastTrack.Query))
                .OnSuccess(releaseAndResults =>
                {
                    var (release, result) = releaseAndResults;
                    var viewModel = _mapper.Map<FastTrackViewModel>(fastTrack);
                    viewModel.FullTable = result;
                    viewModel.Query.PublicationId = release.PublicationId;
                    viewModel.ReleaseSlug = release.Slug;

                    var latestRelease = _releaseRepository.GetLatestPublishedRelease(release.PublicationId);
                    viewModel.LatestData = latestRelease?.Id == release.Id;
                    viewModel.LatestReleaseTitle = latestRelease != null
                        ? TimePeriodLabelFormatter.Format(latestRelease.Year, latestRelease.TimeIdentifier,
                            FullLabelBeforeYear) : null;
                    return viewModel;
                });
        }

        private async Task<Either<ActionResult, FastTrack>> GetFastTrack(Guid id)
        {
            return await GetReleaseFastTrack(id)
                .OnSuccess(async releaseFastTrack =>
                {
                    var text = await _blobStorageService.DownloadBlobText(PublicContent,
                        PublicContentFastTrackPath(releaseFastTrack.ReleaseId.ToString(), id.ToString()));
                    return JsonConvert.DeserializeObject<FastTrack>(text);
                });
        }

        public async Task<Either<ActionResult, ReleaseFastTrack>> GetReleaseFastTrack(Guid fastTrackId)
        {
            // Assume that ReleaseFastTrack has a unique row key across all partitions
            var query = new TableQuery<ReleaseFastTrack>()
                .Where(TableQuery.GenerateFilterCondition(nameof(ReleaseFastTrack.RowKey),
                    QueryComparisons.Equal, fastTrackId.ToString()));

            var tableResult = await _tableStorageService.ExecuteQueryAsync(PublicReleaseFastTrackTableName, query);
            var entity = tableResult.FirstOrDefault();

            return entity == null
                ? new NotFoundResult()
                : new Either<ActionResult, ReleaseFastTrack>(entity);
        }
    }
}
