#nullable enable
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class PermalinkService : IPermalinkService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IPermalinkCsvMetaService _permalinkCsvMetaService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IMapper _mapper;

        public PermalinkService(
            ContentDbContext contentDbContext,
            ITableBuilderService tableBuilderService,
            IPermalinkCsvMetaService permalinkCsvMetaService,
            IBlobStorageService blobStorageService,
            ISubjectRepository subjectRepository,
            IPublicationRepository publicationRepository,
            IReleaseRepository releaseRepository,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _tableBuilderService = tableBuilderService;
            _permalinkCsvMetaService = permalinkCsvMetaService;
            _blobStorageService = blobStorageService;
            _subjectRepository = subjectRepository;
            _publicationRepository = publicationRepository;
            _releaseRepository = releaseRepository;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, LegacyPermalinkViewModel>> Get(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await Find(id, cancellationToken).OnSuccess(BuildViewModel);
        }

        public async Task<Either<ActionResult, Unit>> DownloadCsvToStream(
            Guid id,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            return await Find(id, cancellationToken)
                .OnSuccessCombineWith(permalink => _permalinkCsvMetaService.GetCsvMeta(permalink, cancellationToken))
                .OnSuccessVoid(
                    async tuple =>
                    {
                        var (permalink, meta) = tuple;

                        await using var writer = new StreamWriter(stream, leaveOpen: true);
                        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, leaveOpen: true);

                        await WriteCsvHeaderRow(csv, meta);
                        await WriteCsvRows(csv, permalink.FullTable.Results, meta, cancellationToken);
                    }
                );
        }

        private async Task WriteCsvHeaderRow(CsvWriter csv, PermalinkCsvMetaViewModel meta)
        {
            var headerRow = new ExpandoObject() as IDictionary<string, object>;

            foreach (var header in meta.Headers)
            {
                headerRow[header] = string.Empty;
            }

            csv.WriteDynamicHeader(headerRow as dynamic);

            await csv.NextRecordAsync();
        }

        private async Task WriteCsvRows(
            IWriter csv,
            List<ObservationViewModel> observations,
            PermalinkCsvMetaViewModel meta,
            CancellationToken cancellationToken)
        {
            var locationHeaders = meta.Headers
                .Where(header => LocationCsvUtils.AllCsvColumns().Contains(header))
                .ToHashSet();

            var rows = observations
                .Select(
                    observation => MapCsvRow(
                        observation: observation,
                        meta: meta,
                        locationHeaders: locationHeaders
                    )
                );

            await csv.WriteRecordsAsync(rows, cancellationToken);
        }

        private object MapCsvRow(
            ObservationViewModel observation,
            PermalinkCsvMetaViewModel meta,
            HashSet<string> locationHeaders)
        {
            var timePeriod = observation.GetTimePeriodTuple();

            var row = new ExpandoObject() as IDictionary<string, object>;

            foreach (var header in meta.Headers)
            {
                row[header] = GetCsvRowValue(header, observation, timePeriod, meta, locationHeaders);
            }

            return row;
        }

        private string GetCsvRowValue(
            string header,
            ObservationViewModel observation,
            (int Year, TimeIdentifier TimeIdentifier) timePeriod,
            PermalinkCsvMetaViewModel meta,
            IReadOnlySet<string> locationHeaders)
        {
            if (header == "time_period")
            {
                return TimePeriodLabelFormatter.FormatCsvYear(timePeriod.Year, timePeriod.TimeIdentifier);
            }

            if (header == "time_identifier")
            {
                return timePeriod.TimeIdentifier.GetEnumLabel();
            }

            if (header == "geographic_level")
            {
                return observation.GeographicLevel.GetEnumLabel();
            }

            if (locationHeaders.Contains(header))
            {
                var location = meta.Locations[observation.LocationId];

                if (location.ContainsKey(header))
                {
                    return location[header];
                }
            }

            if (meta.Filters.TryGetValue(header, out var filter))
            {
                var match = observation.Filters
                    .Where(filterItemId => filter.Items.ContainsKey(filterItemId))
                    .OfType<Guid?>()
                    .FirstOrDefault(null as Guid?);

                if (match.HasValue)
                {
                    return filter.Items[match.Value].Label;
                }
            }

            if (meta.Indicators.TryGetValue(header, out var indicator))
            {
                var match = observation.Measures
                    .First(measure => measure.Key == indicator.Id);

                return match.Value;
            }

            return string.Empty;
        }

        private async Task<Either<ActionResult, LegacyPermalink>> Find(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var text = await _blobStorageService.DownloadBlobText(
                    containerName: Permalinks,
                    path: id.ToString(),
                    cancellationToken: cancellationToken);

                return JsonConvert.DeserializeObject<LegacyPermalink>(
                    value: text,
                    settings: BuildJsonSerializerSettings())!;
            }
            catch (FileNotFoundException)
            {
                return new NotFoundResult();
            }
        }

        public async Task<Either<ActionResult, LegacyPermalinkViewModel>> Create(PermalinkCreateViewModel request)
        {
            return await _subjectRepository.FindPublicationIdForSubject(request.Query.SubjectId)
                .OrNotFound()
                .OnSuccess(publicationId => _releaseRepository.GetLatestPublishedRelease(publicationId))
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
                .Include(p => p.LatestPublishedRelease)
                .SingleAsync(p => p.Id == releasesWithSubject.First().PublicationId);

            var latestPublishedRelease = publication.LatestPublishedRelease;

            if (latestPublishedRelease != null && releasesWithSubject.All(r =>
                    r.Year != latestPublishedRelease.Year
                    || r.TimePeriodCoverage != latestPublishedRelease.TimePeriodCoverage))
            {
                return PermalinkStatus.NotForLatestRelease;
            }

            if (latestPublishedRelease != null
                && releasesWithSubject.All(r => r.Id != latestPublishedRelease.Id))
            {
                return PermalinkStatus.SubjectReplacedOrRemoved;
            }

            if (await _publicationRepository.IsSuperseded(publication.Id))
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
