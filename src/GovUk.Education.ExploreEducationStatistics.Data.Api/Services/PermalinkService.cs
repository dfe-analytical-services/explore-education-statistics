#nullable enable
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class PermalinkService : IPermalinkService
    {
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly ContentDbContext _contentDbContext;
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IMapper _mapper;

        public PermalinkService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext contentDbContext,
            ITableBuilderService tableBuilderService,
            IBlobStorageService blobStorageService,
            ISubjectRepository subjectRepository,
            IReleaseRepository releaseRepository,
            IMapper mapper)
        {
            _statisticsDbContext = statisticsDbContext;
            _contentDbContext = contentDbContext;
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
                when ((HttpStatusCode)e.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
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
            var viewModel = _mapper.Map<PermalinkViewModel>(permalink);

            viewModel.Invalidated = !await IsValid(permalink.Query.SubjectId);

            return viewModel;
        }

        private async Task<bool> IsValid(Guid subjectId)
        {
            var subject = await _subjectRepository.Get(subjectId);
            if (subject == null)
            {
                return false;
            }

            var releaseSubject = _statisticsDbContext.ReleaseSubject
                .Include(rs => rs.Release)
                .Where(rs => rs.SubjectId == subjectId)
                .ToList()
                .SingleOrDefault(rs =>
                    _releaseRepository.IsLatestVersionOfRelease(rs.Release.PublicationId, rs.Release.Id));
            if (releaseSubject == null)
            {
                return false;
            }

            var publication = await _contentDbContext.Releases
                .Include(r => r.Publication)
                .ThenInclude(p => p.SupersededBy)
                .Where(r => r.Id == releaseSubject.ReleaseId)
                .Select(r => r.Publication)
                .Distinct()
                .SingleAsync();

            return publication.SupersededById == null
                   || !_contentDbContext.Releases
                       .Include(p => p.Publication)
                       .Where(r => r.PublicationId == publication.SupersededById)
                       .ToList()
                       .Any(r => r.IsLatestPublishedVersionOfRelease());
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
