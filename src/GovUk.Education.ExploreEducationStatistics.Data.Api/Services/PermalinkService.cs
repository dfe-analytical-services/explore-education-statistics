#nullable enable
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
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
        private readonly ContentDbContext _contentDbContext;
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IMapper _mapper;

        public PermalinkService(
            ContentDbContext contentDbContext,
            ITableBuilderService tableBuilderService,
            IBlobStorageService blobStorageService,
            ISubjectRepository subjectRepository,
            IReleaseRepository releaseRepository,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _tableBuilderService = tableBuilderService;
            _blobStorageService = blobStorageService;
            _subjectRepository = subjectRepository;
            _releaseRepository = releaseRepository;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, LegacyPermalinkViewModel>> Get(Guid id)
        {
            try
            {
                var text = await _blobStorageService.DownloadBlobText(Permalinks, id.ToString());
                var permalink = JsonConvert.DeserializeObject<LegacyPermalink>(
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

        public async Task<Either<ActionResult, LegacyPermalinkViewModel>> Create(PermalinkCreateViewModel request)
        {
            var publicationId = await _subjectRepository.GetPublicationIdForSubject(request.Query.SubjectId);
            return await _releaseRepository.GetLatestPublishedRelease(publicationId)
                .OnSuccess(release => Create(release.Id, request));
        }

        public async Task<Either<ActionResult, LegacyPermalinkViewModel>> Create(Guid releaseId,
            PermalinkCreateViewModel request)
        {
            return await _tableBuilderService.Query(releaseId, request.Query)
                .OnSuccess(async result =>
                {
                    var permalinkTableResult = new PermalinkTableBuilderResult(result);
                    var permalink = new Permalink
                    {
                        Created = DateTime.UtcNow,
                        PublicationTitle = result.SubjectMeta.PublicationName,
                        DataSetTitle = result.SubjectMeta.SubjectName,
                        ReleaseId = releaseId,
                        SubjectId = request.Query.SubjectId
                    };

                    _contentDbContext.Permalinks.Add(permalink);
                    await _contentDbContext.SaveChangesAsync();

                    var legacyPermalink = new LegacyPermalink(
                        permalink.Id,
                        permalink.Created,
                        request.Configuration,
                        permalinkTableResult,
                        request.Query);

                    await _blobStorageService.UploadAsJson(containerName: Permalinks,
                        path: permalink.Id.ToString(),
                        content: legacyPermalink,
                        settings: BuildJsonSerializerSettings());

                    return await BuildViewModel(legacyPermalink);
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

        private async Task<LegacyPermalinkViewModel> BuildViewModel(LegacyPermalink permalink)
        {
            var viewModel = _mapper.Map<LegacyPermalinkViewModel>(permalink);

            viewModel.Status = await GetPermalinkStatus(permalink.Query.SubjectId);

            return viewModel;
        }

        private async Task<PermalinkStatus> GetPermalinkStatus(Guid subjectId)
        {
            // TODO EES-3339 This doesn't currently include a status to warn if the footnotes have been amended on a Release,
            // and will return 'Current' unless one of the other cases also applies.

            var releasesWithSubject = await _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Include(rf => rf.Release)
                .Where(rf =>
                    rf.File.SubjectId == subjectId
                    && rf.File.Type == FileType.Data
                    && rf.Release.Published.HasValue && DateTime.UtcNow >= rf.Release.Published.Value)
                .Select(rf => rf.Release)
                .ToListAsync();

            if (releasesWithSubject.Count == 0)
            {
                return PermalinkStatus.SubjectRemoved;
            }

            var publication = await _contentDbContext.Publications
                .Include(p => p.Releases)
                .SingleAsync(p => p.Id == releasesWithSubject.First().PublicationId);

            var latestRelease = publication.LatestPublishedRelease();

            if (latestRelease != null && releasesWithSubject.All(r =>
                    r.Year != latestRelease.Year
                    || r.TimePeriodCoverage != latestRelease.TimePeriodCoverage))
            {
                return PermalinkStatus.NotForLatestRelease;
            }

            if (latestRelease != null
                && releasesWithSubject.All(r => r.Id != latestRelease.Id))
            {
                return PermalinkStatus.SubjectReplacedOrRemoved;
            }

            if (publication.SupersededById != null
                && (await _contentDbContext.Releases
                    .Include(p => p.Publication)
                    .Where(r => r.PublicationId == publication.SupersededById)
                    .ToListAsync())
                .Any(r => r.IsLatestPublishedVersionOfRelease()))
            {
                return PermalinkStatus.PublicationSuperseded;
            }

            return PermalinkStatus.Current;
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
