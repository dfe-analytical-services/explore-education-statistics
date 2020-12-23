using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using StorageException = Microsoft.Azure.Storage.StorageException;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FastTrackService : IFastTrackService
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ISubjectService _subjectService;
        private readonly ITableStorageService _tableStorageService;
        private readonly IMapper _mapper;

        public FastTrackService(
            ITableBuilderService tableBuilderService,
            IBlobStorageService blobStorageService,
            ISubjectService subjectService,
            ITableStorageService tableStorageService,
            IMapper mapper)
        {
            _tableBuilderService = tableBuilderService;
            _blobStorageService = blobStorageService;
            _subjectService = subjectService;
            _tableStorageService = tableStorageService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, FastTrackViewModel>> Get(Guid id)
        {
            try
            {
                return await GetFastTrack(id)
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
            return _tableBuilderService.Query(fastTrack.ReleaseId, fastTrack.Query).OnSuccess(result =>
            {
                var viewModel = _mapper.Map<FastTrackViewModel>(fastTrack);
                viewModel.FullTable = result;
                viewModel.Query.PublicationId =
                    _subjectService.GetPublicationForSubject(fastTrack.Query.SubjectId).Result.Id;
                return viewModel;
            });
        }

        private async Task<Either<ActionResult, FastTrack>> GetFastTrack(Guid id)
        {
            return await GetReleaseFastTrack(id)
                .OnSuccess(async releaseFastTrack =>
                {
                    var text = await _blobStorageService.DownloadBlobText(PublicContentContainerName,
                        PublicContentFastTrackPath(releaseFastTrack.ReleaseId.ToString(), id.ToString()));
                    return JsonConvert.DeserializeObject<FastTrack>(text);
                });
        }

        private async Task<Either<ActionResult, ReleaseFastTrack>> GetReleaseFastTrack(Guid id)
        {
            // Assume that ReleaseFastTrack has a unique row key across all partitions
            var query = new TableQuery<ReleaseFastTrack>()
                .Where(TableQuery.GenerateFilterCondition(nameof(ReleaseFastTrack.RowKey),
                    QueryComparisons.Equal, id.ToString()));

            var tableResult = await _tableStorageService.ExecuteQueryAsync(PublicReleaseFastTrackTableName, query);
            var entity = tableResult.FirstOrDefault();

            return entity == null
                ? new NotFoundResult()
                : new Either<ActionResult, ReleaseFastTrack>(entity);
        }
    }
}