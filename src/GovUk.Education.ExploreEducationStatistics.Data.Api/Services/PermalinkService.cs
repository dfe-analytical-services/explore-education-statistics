#nullable enable
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
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
using Newtonsoft.Json;
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
        private readonly IFrontendService _frontendService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IMapper _mapper;

        public PermalinkService(
            ContentDbContext contentDbContext,
            ITableBuilderService tableBuilderService,
            IPermalinkCsvMetaService permalinkCsvMetaService,
            IBlobStorageService blobStorageService,
            IFrontendService frontendService,
            ISubjectRepository subjectRepository,
            IPublicationRepository publicationRepository,
            IReleaseRepository releaseRepository,
            IMapper mapper)
        {
            _contentDbContext = contentDbContext;
            _tableBuilderService = tableBuilderService;
            _permalinkCsvMetaService = permalinkCsvMetaService;
            _blobStorageService = blobStorageService;
            _frontendService = frontendService;
            _subjectRepository = subjectRepository;
            _publicationRepository = publicationRepository;
            _releaseRepository = releaseRepository;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> GetPermalink(Guid permalinkId,
            CancellationToken cancellationToken = default)
        {
            return await Find(permalinkId, cancellationToken).OnSuccess(BuildViewModel);
        }

        private async Task<Either<ActionResult, Permalink>> Find(Guid permalinkId,
            CancellationToken cancellationToken)
        {
            return await _contentDbContext.Permalinks
                .SingleOrNotFoundAsync(
                    predicate: permalink => permalink.Id == permalinkId &&
                                            (!permalink.Legacy || permalink.LegacyHasSnapshot == true),
                    cancellationToken: cancellationToken)
                .OnSuccess<ActionResult, Permalink, Permalink>(async permalink =>
                {
                    try
                    {
                        var tableJson = await _blobStorageService.DownloadBlobText(
                            containerName: BlobContainers.PermalinkSnapshots,
                            path: $"{permalink.Id}.json",
                            cancellationToken: cancellationToken);

                        permalink.Table = JsonConvert.DeserializeObject<dynamic>(tableJson);
                        return permalink;
                    }
                    catch (FileNotFoundException)
                    {
                        return new NotFoundResult();
                    }
                });
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> CreatePermalink(PermalinkCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await _subjectRepository.FindPublicationIdForSubject(request.Query.SubjectId)
                .OrNotFound()
                .OnSuccess(publicationId => _releaseRepository.GetLatestPublishedRelease(publicationId))
                .OnSuccess(release => CreatePermalink(release.Id, request, cancellationToken));
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> CreatePermalink(Guid releaseId,
            PermalinkCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            return await _tableBuilderService.Query(releaseId, request.Query, cancellationToken)
                .OnSuccessCombineWith(tableResult => _frontendService.CreateUniversalTable(tableResult,
                    request.Configuration,
                    cancellationToken))
                .OnSuccessCombineWith(async tuple =>
                {
                    var (tableResult, _) = tuple;
                    return await _permalinkCsvMetaService.GetCsvMeta(request.Query.SubjectId,
                        tableResult.SubjectMeta.Locations,
                        tableResult.SubjectMeta.Filters,
                        tableResult.SubjectMeta.Indicators,
                        cancellationToken);
                })
                .OnSuccess(async tuple =>
                {
                    var (tableResult, universalTable, csvMeta) = tuple;
                    var subjectMeta = tableResult.SubjectMeta;
                    var permalink = new Permalink
                    {
                        ReleaseId = releaseId,
                        SubjectId = request.Query.SubjectId,
                        PublicationTitle = subjectMeta.PublicationName,
                        DataSetTitle = subjectMeta.SubjectName,
                        CountFilterItems = CountFilterItems(subjectMeta.Filters),
                        CountFootnotes = subjectMeta.Footnotes.Count,
                        CountIndicators = subjectMeta.Indicators.Count,
                        CountLocations = CountLocations(subjectMeta.Locations),
                        CountObservations = tableResult.Results.Count(),
                        CountTimePeriods = subjectMeta.TimePeriodRange.Count,
                        Table = universalTable
                    };
                    _contentDbContext.Permalinks.Add(permalink);
                    await UploadSnapshot(permalink, tableResult.Results.ToList(), csvMeta, cancellationToken);
                    await _contentDbContext.SaveChangesAsync(cancellationToken);
                    return await BuildViewModel(permalink);
                });
        }

        public Task<Either<ActionResult, Unit>> DownloadCsvToStream(Guid permalinkId,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            return Find(permalinkId, cancellationToken)
                .OnSuccessVoid(() => _blobStorageService.DownloadToStream(
                    containerName: BlobContainers.PermalinkSnapshots,
                    path: $"{permalinkId}.csv",
                    stream: stream
                ));
        }

        // TODO EES-3755 Remove after Permalink snapshot work is complete
        public async Task<Either<ActionResult, LegacyPermalinkViewModel>> GetLegacy(
            Guid permalinkId,
            CancellationToken cancellationToken = default)
        {
            return await FindLegacy(permalinkId, cancellationToken).OnSuccess(BuildLegacyViewModel);
        }

        // TODO EES-3755 Remove after Permalink snapshot work is complete
        public async Task<Either<ActionResult, Unit>> LegacyDownloadCsvToStream(
            Guid permalinkId,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            return await FindLegacy(permalinkId, cancellationToken)
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
            Guid permalinkId,
            CancellationToken cancellationToken)
        {
            return await _contentDbContext.Permalinks
                .SingleOrNotFoundAsync(
                    predicate: permalink => permalink.Id == permalinkId &&
                                            permalink.Legacy == true,
                    cancellationToken: cancellationToken)
                .OnSuccess<ActionResult, Permalink, LegacyPermalink>(async () =>
                {
                    try
                    {
                        var text = await _blobStorageService.DownloadBlobText(
                            containerName: BlobContainers.Permalinks,
                            path: permalinkId.ToString(),
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
            return await _tableBuilderService.Query(releaseId, request.Query)
                .OnSuccess(async result =>
                {
                    var permalinkTableResult = new PermalinkTableBuilderResult(result);
                    var subjectMeta = permalinkTableResult.SubjectMeta;
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
                        CountLocations = CountLocations(subjectMeta.LocationsHierarchical),
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

        private async Task<PermalinkViewModel> BuildViewModel(Permalink permalink)
        {
            var viewModel = _mapper.Map<PermalinkViewModel>(permalink);
            viewModel.Status = await GetPermalinkStatus(permalink.SubjectId);

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

        private Task UploadSnapshot(Permalink permalink,
            List<ObservationViewModel> observations,
            PermalinkCsvMetaViewModel csvMeta,
            CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(
                UploadTableCsv(permalink, observations, csvMeta, cancellationToken),
                UploadTableJson(permalink, cancellationToken)
            );
        }

        private async Task UploadTableCsv(Permalink permalink,
            List<ObservationViewModel> observations,
            PermalinkCsvMetaViewModel csvMeta,
            CancellationToken cancellationToken = default)
        {
            await using var csvStream = new MemoryStream();
            await using var csvWriter = new CsvWriter(new StreamWriter(csvStream, leaveOpen: true),
                CultureInfo.InvariantCulture);
            await WriteCsvHeaderRow(csvWriter, csvMeta);
            await WriteCsvRows(csvWriter, observations, csvMeta, cancellationToken);
            await csvWriter.FlushAsync();

            await _blobStorageService.UploadStream(
                containerName: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.csv",
                stream: csvStream,
                contentType: "text/csv",
                cancellationToken: cancellationToken
            );
        }

        private async Task UploadTableJson(Permalink permalink,
            CancellationToken cancellationToken = default)
        {
            await using var tableStream = new MemoryStream();
            await using var jsonWriter = new JsonTextWriter(new StreamWriter(tableStream, leaveOpen: true));
            JsonSerializer.CreateDefault().Serialize(jsonWriter, permalink.Table);
            await jsonWriter.FlushAsync(cancellationToken);

            await _blobStorageService.UploadStream(
                containerName: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json",
                stream: tableStream,
                contentType: MediaTypeNames.Application.Json,
                cancellationToken: cancellationToken
            );
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
