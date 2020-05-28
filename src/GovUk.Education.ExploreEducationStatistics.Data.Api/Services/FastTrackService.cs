using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Azure.Storage;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class FastTrackService : IFastTrackService
    {
        private const string ContainerName = "fasttrack";

        private readonly ITableBuilderService _tableBuilderService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ISubjectService _subjectService;
        private readonly IReleaseService _releaseService;
        private readonly IMapper _mapper;

        public FastTrackService(
            ITableBuilderService tableBuilderService,
            IFileStorageService fileStorageService,
            ISubjectService subjectService,
            IReleaseService releaseService,
            IMapper mapper)
        {
            _tableBuilderService = tableBuilderService;
            _fileStorageService = fileStorageService;
            _subjectService = subjectService;
            _releaseService = releaseService;
            _mapper = mapper;
        }
        
        public async Task<Either<ActionResult, FastTrackViewModel>> GetAsync(Guid fastTrackId)
        {
            try
            {
                var fastTrack = await GetFastTrack(fastTrackId.ToString());
                var subjectId = fastTrack.Query.SubjectId;
                var publicationId = _subjectService.GetPublicationForSubjectAsync(subjectId).Result.Id;
                var releaseId = _releaseService.GetLatestPublishedRelease(publicationId);
                return await BuildViewModel(releaseId.Value, fastTrack);
            }
            catch (StorageException e)
                when ((HttpStatusCode) e.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        public async Task<Either<ActionResult, FastTrackViewModel>> GetAsync(Guid releaseId, Guid fastTrackId)
        {
            try
            {
                var fastTrack = await GetFastTrack(fastTrackId.ToString());
                return await BuildViewModel(releaseId, fastTrack);
            }
            catch (StorageException e)
                when ((HttpStatusCode) e.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        private Task<Either<ActionResult, FastTrackViewModel>> BuildViewModel(Guid releaseId, FastTrack fastTrack)
        {
            return _tableBuilderService.Query(releaseId, fastTrack.Query).OnSuccess(result =>
            {
                var viewModel = _mapper.Map<FastTrackViewModel>(fastTrack);
                viewModel.FullTable = result;
                viewModel.Query.PublicationId = 
                    _subjectService.GetPublicationForSubjectAsync(fastTrack.Query.SubjectId).Result.Id;
                return viewModel;
            });
        }

        private async Task<FastTrack> GetFastTrack(string fastTrackId)
        {
            var text = await _fileStorageService.DownloadTextAsync(ContainerName, fastTrackId);
            return JsonConvert.DeserializeObject<FastTrack>(text);
        }
    }
}