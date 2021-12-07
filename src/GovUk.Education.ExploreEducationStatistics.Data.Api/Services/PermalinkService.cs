#nullable enable
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Extensions.Options;
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
        private readonly LocationsOptions _locationOptions;
        private readonly IMapper _mapper;

        public PermalinkService(ITableBuilderService tableBuilderService,
            IBlobStorageService blobStorageService,
            ISubjectRepository subjectRepository,
            IReleaseRepository releaseRepository,
            IOptions<LocationsOptions> locationOptions,
            IMapper mapper)
        {
            _tableBuilderService = tableBuilderService;
            _blobStorageService = blobStorageService;
            _subjectRepository = subjectRepository;
            _releaseRepository = releaseRepository;
            _locationOptions = locationOptions.Value;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> Get(Guid id)
        {
            try
            {
                var text = await _blobStorageService.DownloadBlobText(Permalinks, id.ToString());
                var permalink = JsonConvert.DeserializeObject<Permalink>(
                    value: text,
                    settings: new JsonSerializerSettings
                    {
                        ContractResolver =
                            new PermalinkContractResolver(_locationOptions.TableResultLocationHierarchiesEnabled)
                    });
                return await BuildViewModel(permalink);
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
                var permalink = new Permalink(request.Configuration, result, request.Query);
                await _blobStorageService.UploadAsJson(containerName: Permalinks,
                    path: permalink.Id.ToString(),
                    content: permalink,
                    settings: new JsonSerializerSettings
                    {
                        ContractResolver =
                            new PermalinkContractResolver(_locationOptions.TableResultLocationHierarchiesEnabled)
                    });
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

    internal class PermalinkContractResolver : DefaultContractResolver
    {
        private readonly bool _tableResultLocationHierarchiesEnabled;

        public PermalinkContractResolver(bool tableResultLocationHierarchiesEnabled)
        {
            _tableResultLocationHierarchiesEnabled = tableResultLocationHierarchiesEnabled;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property.DeclaringType == typeof(Permalink))
            {
                property.ShouldSerialize = property.PropertyName switch
                {
                    "Locations" => _ => !_tableResultLocationHierarchiesEnabled,
                    "LocationsHierarchical" => _ => _tableResultLocationHierarchiesEnabled,
                    _ => property.ShouldSerialize
                };
            }

            return property;
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            JsonObjectContract contract = base.CreateObjectContract(objectType);

            if (_tableResultLocationHierarchiesEnabled && objectType == typeof(ResultSubjectMetaViewModel))
            {
                contract.Converter = new ResultSubjectMetaViewModelJsonConverter();
            }

            return contract;
        }
    }
}
