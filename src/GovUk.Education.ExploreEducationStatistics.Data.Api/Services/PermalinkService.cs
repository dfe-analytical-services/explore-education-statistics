#nullable enable
using System;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
                var permalink = JsonConvert.DeserializeObject<Permalink>(
                    value: text,
                    settings: BuildJsonSerializerSettings());
                return await BuildViewModel(permalink!);
            }
            catch (StorageException e)
                when ((HttpStatusCode) e.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return new NotFoundResult();
            }
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> Create(PermalinkCreateViewModel request)
        {
            var publicationId = await _subjectRepository.GetPublicationIdForSubject(request.Query.SubjectId);
            var release = _releaseRepository.GetLatestPublishedRelease(publicationId);

            if (release == null)
            {
                return new NotFoundResult();
            }

            return await Create(release.Id, request);
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> Create(Guid releaseId,
            PermalinkCreateViewModel request)
        {
            return await _tableBuilderService.Query(releaseId, request.Query).OnSuccess(async result =>
            {
                var permalinkTableResult = new PermalinkTableBuilderResult(result);
                var permalink = new Permalink(request.Configuration, permalinkTableResult, request.Query);
                await _blobStorageService.UploadAsJson(containerName: Permalinks,
                    path: permalink.Id.ToString(),
                    content: permalink,
                    settings: BuildJsonSerializerSettings());
                return await BuildViewModel(permalink);
            });
        }

        private static JsonSerializerSettings BuildJsonSerializerSettings()
        {
            return new()
            {
                ContractResolver = new PermalinkContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
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

    internal class PermalinkContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            JsonObjectContract contract = base.CreateObjectContract(objectType);

            if (objectType == typeof(PermalinkResultSubjectMeta))
            {
                contract.Converter = new PermalinkResultSubjectMetaJsonConverter();
            }

            return contract;
        }
    }
}
