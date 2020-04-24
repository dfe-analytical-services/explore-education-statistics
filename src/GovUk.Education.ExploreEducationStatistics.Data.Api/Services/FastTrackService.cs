using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
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
        private readonly IMapper _mapper;

        public FastTrackService(
            ITableBuilderService tableBuilderService,
            IFileStorageService fileStorageService,
            ISubjectService subjectService,
            IMapper mapper)
        {
            _tableBuilderService = tableBuilderService;
            _fileStorageService = fileStorageService;
            _subjectService = subjectService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, FastTrackViewModel>> GetAsync(Guid fastTrackId)
        {
            try
            {
                var text = await _fileStorageService.DownloadTextAsync(ContainerName, fastTrackId.ToString());
                var fastTrack = JsonConvert.DeserializeObject<FastTrack>(text);
                return await BuildViewModel(fastTrack);
            }
            catch (StorageException e)
                when ((HttpStatusCode) e.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        private Task<Either<ActionResult, FastTrackViewModel>> BuildViewModel(FastTrack fastTrack)
        {
            return _tableBuilderService.Query(fastTrack.Query).OnSuccess(result =>
            {
                var viewModel = _mapper.Map<FastTrackViewModel>(fastTrack);
                viewModel.FullTable = result;
                viewModel.Query.PublicationId = 
                    _subjectService.GetPublicationForSubjectAsync(fastTrack.Query.SubjectId).Result.Id;
                return viewModel;
            });
        }
    }
}