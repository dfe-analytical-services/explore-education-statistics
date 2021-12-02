#nullable enable
using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class PermalinkService : IPermalinkService
    {
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IMapper _mapper;

        public PermalinkService(ITableBuilderService tableBuilderService,
            IBlobStorageService blobStorageService,
            ISubjectRepository subjectRepository,
            IReleaseRepository releaseRepository,
            IMapper mapper)
        {
            _tableBuilderService = tableBuilderService;
            _blobStorageService = blobStorageService;
            _subjectRepository = subjectRepository;
            _releaseRepository = releaseRepository;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> Get(Guid id)
        {
            try
            {
                var text = await _blobStorageService.DownloadBlobText(Permalinks, id.ToString());
                var permalink = JsonConvert.DeserializeObject<Permalink>(text);
                return await BuildViewModel(permalink);
            }
            catch (StorageException e)
                when ((HttpStatusCode) e.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> Create(CreatePermalinkRequest request)
        {
            var publicationId = _subjectRepository.GetPublicationIdForSubject(request.Query.SubjectId).Result;
            var release = _releaseRepository.GetLatestPublishedRelease(publicationId);

            if (release == null)
            {
                return new NotFoundResult();
            }

            return await Create(release.Id, request);
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> Create(Guid releaseId,
            CreatePermalinkRequest request)
        {
            return await _tableBuilderService.Query(releaseId, request.Query).OnSuccess(async result =>
            {
                var permalink = new Permalink(request.Configuration, result, request.Query);
                await _blobStorageService.UploadText(containerName: Permalinks,
                    path: permalink.Id.ToString(),
                    content: JsonConvert.SerializeObject(permalink),
                    contentType: MediaTypeNames.Application.Json);
                return await BuildViewModel(permalink);
            });
        }

        private async Task<PermalinkViewModel> BuildViewModel(Permalink permalink)
        {
            var subject = await _subjectRepository.Get(permalink.Query.SubjectId);
            var isValid = subject != null && await _subjectRepository.IsSubjectForLatestPublishedRelease(subject.Id);

            var publicationId = await _subjectRepository.FindPublicationIdForSubject(permalink.Query.SubjectId);

            var viewModel = _mapper.Map<PermalinkViewModel>(permalink);

            viewModel.Query.PublicationId = publicationId;
            viewModel.Invalidated = !isValid;

            return viewModel;
        }
    }
}
