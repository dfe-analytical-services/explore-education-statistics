#nullable enable
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Converters;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PermalinkService> _logger;

        public PermalinkService(
            ContentDbContext contentDbContext,
            ITableBuilderService tableBuilderService,
            IPermalinkCsvMetaService permalinkCsvMetaService,
            IBlobStorageService blobStorageService,
            ISubjectRepository subjectRepository,
            IPublicationRepository publicationRepository,
            IReleaseRepository releaseRepository,
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            HttpClient httpClient,
            ILogger<PermalinkService> logger)
        {
            _contentDbContext = contentDbContext;
            _tableBuilderService = tableBuilderService;
            _permalinkCsvMetaService = permalinkCsvMetaService;
            _blobStorageService = blobStorageService;
            _subjectRepository = subjectRepository;
            _publicationRepository = publicationRepository;
            _releaseRepository = releaseRepository;
            _mapper = mapper;
            _contentPersistenceHelper = contentPersistenceHelper;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Either<ActionResult, PermalinkSnapshotViewModel>> Get(Guid id,
            CancellationToken cancellationToken = default)
        {
            var subjectId = await _contentDbContext.Permalinks
                .Where(permalink => permalink.Id == id)
                .Select(permalink => permalink.SubjectId)
                .FirstOrDefaultAsync(cancellationToken);

            // do we want to return not found here?
            // can a permalink exist without an existing subject? 
            // assume not but worth double checking
            if (subjectId == null)
            {
                return new NotFoundResult();
            }

            return await Find(id, cancellationToken)
                .OnSuccess(
                    async permalink =>
                        await BuildPermalinkSnapshotViewModel(
                            permalink with { Status = await GetPermalinkStatus(permalink.Id) }, subjectId));
        }

        private async Task<Either<ActionResult, PermalinkSnapshotViewModel>?> Find(Guid permalinkId,
            CancellationToken cancellationToken)
        {
            try
            {
                await _contentPersistenceHelper.CheckEntityExists<PermalinkSnapshotViewModel>(permalinkId).OnSuccess(
                        async permalink =>
                        {
                            var text = await _blobStorageService.DownloadBlobText(
                                containerName: BlobContainers.PermalinkSnapshots,
                                path: permalink.Id.ToString(),
                                cancellationToken: cancellationToken
                            );

                            return JsonConvert.DeserializeObject<PermalinkSnapshotViewModel>(
                                value: text
                            );
                        })
                    .OrNotFound();
            }
            catch (FileNotFoundException)
            {
                return new NotFoundResult();
            }

            return null;
        }

        public async Task<Either<ActionResult, PermalinkSnapshotViewModel>> Create(PermalinkCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await _subjectRepository.FindPublicationIdForSubject(request.Query.SubjectId)
                .OrNotFound()
                .OnSuccess(publicationId => _releaseRepository.GetLatestPublishedRelease(publicationId))
                .OnSuccess(release => Create(release.Id, request, cancellationToken));
        }

        public async Task<Either<ActionResult, PermalinkSnapshotViewModel>> Create(Guid releaseId,
            PermalinkCreateRequest request, CancellationToken cancellationToken = default)
        {
            return _tableBuilderService.Query(releaseId, request.Query, cancellationToken)
                .OnSuccess(async result =>
                {
                    var permalinkTableCreateRequest = new PermalinkTableCreateRequest
                    {
                        FullTable = new TableBuilderResultViewModel
                        {
                            Results = result.Results.ToList(),
                            SubjectMeta = result.SubjectMeta
                        },
                        Configuration = request.Configuration
                    };

                    var camelCaseJsonSerializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Ignore
                    };

                    _logger.LogInformation("Calling frontend API endpoint to create permalink");

                    var response = await _httpClient.PostAsync("http://localhost:3000/api/permalink",
                        new JsonNetContent(permalinkTableCreateRequest, camelCaseJsonSerializerSettings),
                        cancellationToken);

                    response.EnsureSuccessStatusCode();

                    _logger.LogInformation("sent following json to frontend: " +
                                           JsonConvert.SerializeObject(permalinkTableCreateRequest));

                    _logger.LogInformation("Got response from frontend:" + response.StatusCode);

                    var content = await response.Content.ReadAsStringAsync(cancellationToken);

                    dynamic json = JToken.Parse(content);

                    _logger.LogInformation("Got response from frontend:" + response.Content);

                    var permalink = new Permalink
                    {
                        Created = DateTime.UtcNow,
                        PublicationTitle = result.SubjectMeta.PublicationName,
                        DataSetTitle = result.SubjectMeta.SubjectName,
                        ReleaseId = releaseId,
                        SubjectId = request.Query.SubjectId
                    };

                    _contentDbContext.Permalinks.Add(permalink);

                    await _permalinkCsvMetaService.GetCsvMeta(request.Query.SubjectId,
                        result.SubjectMeta.Locations,
                        result.SubjectMeta.Filters,
                        result.SubjectMeta.Indicators,
                        cancellationToken).OnSuccessVoid(async meta =>
                    {
                        // TODO - think about not loading the whole thing into memory & stream it instead
                        var csvMemoryStream = new MemoryStream();

                        await using var writer = new StreamWriter(csvMemoryStream, leaveOpen: true);
                        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                        await WriteCsvHeaderRow(csv, meta);
                        await WriteCsvRows(csv, result.Results.ToList(), meta, cancellationToken);

                        // CSV
                        await _blobStorageService.UploadStream(
                            containerName: BlobContainers.PermalinkSnapshots,
                            path: permalink.Id.ToString(),
                            stream: csvMemoryStream,
                            contentType: MediaTypeNames.Text.Plain
                        );

                        var jsonStream = new MemoryStream(json);

                        // Universal table format
                        await _blobStorageService.UploadStream(
                            containerName: BlobContainers.PermalinkSnapshots,
                            path: permalink.Id.ToString(),
                            stream: jsonStream,
                            contentType: MediaTypeNames.Application.Json
                        );

                        await _contentDbContext.SaveChangesAsync(cancellationToken);
                    });

                    return new PermalinkSnapshotViewModel
                    {
                        Id = permalink.Id,
                        Created = permalink.Created,
                        Status = await GetPermalinkStatus(permalink.SubjectId),
                        Table = json
                    };
                });
        }

        public Task<Either<ActionResult, Stream>> StreamPermalinkCsv(Guid id, Stream stream,
            CancellationToken cancellationToken = default)
        {
            // Look in DB here (pt) first to avoid downloading the blob if we don't need to
            return _blobStorageService.DownloadToStream(
                containerName: BlobContainers.PermalinkSnapshots,
                path: id.ToString(),
                stream: new MemoryStream()
            );
        }

        // TODO EES-3755 Remove after Permalink snapshot work is complete
        public async Task<Either<ActionResult, LegacyPermalinkViewModel>> GetLegacy(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await FindLegacy(id, cancellationToken).OnSuccess(BuildLegacyViewModel);
        }

        // TODO EES-3755 Remove after Permalink snapshot work is complete
        public async Task<Either<ActionResult, Unit>> LegacyDownloadCsvToStream(
            Guid id,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            return await FindLegacy(id, cancellationToken)
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
            switch (header)
            {
                case "time_period":
                    return TimePeriodLabelFormatter.FormatCsvYear(timePeriod.Year, timePeriod.TimeIdentifier);
                case "time_identifier":
                    return timePeriod.TimeIdentifier.GetEnumLabel();
                case "geographic_level":
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

        // TODO EES-3755 Remove after Permalink snapshot work is complete
        private async Task<Either<ActionResult, LegacyPermalink>> FindLegacy(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _contentDbContext.Permalinks
                .SingleOrNotFoundAsync(
                    predicate: permalink => permalink.Id == id &&
                                            permalink.Legacy == true,
                    cancellationToken: cancellationToken)
                .OnSuccess<ActionResult, Permalink, LegacyPermalink>(async () =>
                {
                    try
                    {
                        var text = await _blobStorageService.DownloadBlobText(
                            containerName: BlobContainers.Permalinks,
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
                });
        }

        // TODO EES-3755 Remove after Permalink snapshot work is complete
        public async Task<Either<ActionResult, LegacyPermalinkViewModel>> CreateLegacy(PermalinkCreateRequest request)
        {
            return await _subjectRepository.FindPublicationIdForSubject(request.Query.SubjectId)
                .OrNotFound()
                .OnSuccess(publicationId => _releaseRepository.GetLatestPublishedRelease(publicationId))
                .OnSuccess(release => CreateLegacy(release.Id, request));
        }

        // TODO EES-3755 Remove after Permalink snapshot work is complete
        public async Task<Either<ActionResult, LegacyPermalinkViewModel>> CreateLegacy(Guid releaseId,
            PermalinkCreateRequest request)
        {
            return _tableBuilderService.Query(releaseId, request.Query)
                .OnSuccess(async result =>
                {
                    var subjectMeta = result.SubjectMeta;
                    var permalinkTableResult = new PermalinkTableBuilderResult(result);
                    var permalink = new Permalink
                    {
                        Legacy = true,
                        PublicationTitle = subjectMeta.PublicationName,
                        DataSetTitle = subjectMeta.SubjectName,
                        ReleaseId = releaseId,
                        SubjectId = request.Query.SubjectId,
                        CountFilterItems = CountFilterItems(subjectMeta.Filters),
                        CountFootnotes = subjectMeta.Footnotes.Count,
                        CountIndicators = subjectMeta.Indicators.Count,
                        CountLocations =
                            CountLocations(subjectMeta
                                .LocationsHierarchical), // due to ambiguous invocation of onSuccess
                        CountObservations = result.Results.Count(),
                        CountTimePeriods = subjectMeta.TimePeriodRange.Count,
                        LegacyHasConfigurationHeaders = true
                    };

                    _contentDbContext.Permalinks.Add(permalink);

                    var legacyPermalink = new LegacyPermalink(
                        permalink.Id,
                        permalink.Created,
                        request.Configuration,
                        permalinkTableResult,
                        request.Query);

                    var content = JsonConvert.SerializeObject(legacyPermalink,
                        BuildJsonSerializerSettings());

                    await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                    permalink.LegacyContentLength = stream.Length;

                    await _blobStorageService.UploadStream(
                        containerName: BlobContainers.Permalinks,
                        path: permalink.Id.ToString(),
                        stream: stream,
                        contentType: MediaTypeNames.Application.Json);

                    await _contentDbContext.SaveChangesAsync();

                    return await BuildLegacyViewModel(legacyPermalink);
                });
        }

        private static int CountFilterItems(Dictionary<string, FilterMetaViewModel> filters)
        {
            return filters.Values.Sum(filter =>
                filter.Options.Values.Sum(filterGroup =>
                    filterGroup.Options.Count));
        }

        private static int CountLocations(
            Dictionary<string, List<LocationAttributeViewModel>> subjectMetaLocationsHierarchical)
        {
            var locationAttributes = subjectMetaLocationsHierarchical.Values
                .SelectMany(locationAttributes => locationAttributes);

            return CountLocations(locationAttributes);
        }

        private static int CountLocations(IEnumerable<LocationAttributeViewModel> locationAttributes)
        {
            return locationAttributes.Sum(
                attribute => attribute.Options is null ? 1 : CountLocations(attribute.Options));
        }

        private static JsonSerializerSettings BuildJsonSerializerSettings()
        {
            return new()
            {
                ContractResolver = new PermalinkContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        private async Task<PermalinkSnapshotViewModel> BuildPermalinkSnapshotViewModel(
            PermalinkSnapshotViewModel permalinkSnapshot,
            Guid subjectId)
        {
            var viewModel = _mapper.Map<PermalinkSnapshotViewModel>(permalinkSnapshot);

            viewModel.Status = await GetPermalinkStatus(subjectId);

            return viewModel;
        }

        // TODO EES-3755 Remove after Permalink snapshot work is complete
        private async Task<LegacyPermalinkViewModel> BuildLegacyViewModel(LegacyPermalink permalink)
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
                    && rf.Release.Published.HasValue && DateTime.UtcNow >= rf.Release.Published.Value
                )
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
            var contract = base.CreateObjectContract(objectType);

            if (objectType == typeof(PermalinkResultSubjectMeta))
            {
                contract.Converter = new PermalinkResultSubjectMetaJsonConverter();
            }

            return contract;
        }
    }
}
