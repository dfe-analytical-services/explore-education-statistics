#nullable enable
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class PermalinkService : IPermalinkService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly ITableBuilderService _tableBuilderService;
        private readonly IPermalinkCsvMetaService _permalinkCsvMetaService;
        private readonly IPublicBlobStorageService _publicBlobStorageService;
        private readonly IFrontendService _frontendService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IReleaseRepository _releaseRepository;

        public PermalinkService(
            ContentDbContext contentDbContext,
            ITableBuilderService tableBuilderService,
            IPermalinkCsvMetaService permalinkCsvMetaService,
            IPublicBlobStorageService publicBlobStorageService,
            IFrontendService frontendService,
            ISubjectRepository subjectRepository,
            IPublicationRepository publicationRepository,
            IReleaseRepository releaseRepository)
        {
            _contentDbContext = contentDbContext;
            _tableBuilderService = tableBuilderService;
            _permalinkCsvMetaService = permalinkCsvMetaService;
            _publicBlobStorageService = publicBlobStorageService;
            _frontendService = frontendService;
            _subjectRepository = subjectRepository;
            _publicationRepository = publicationRepository;
            _releaseRepository = releaseRepository;
        }

        public async Task<Either<ActionResult, PermalinkViewModel>> GetPermalink(Guid permalinkId,
            CancellationToken cancellationToken = default)
        {
            return await Find(permalinkId, cancellationToken)
                .OnSuccessCombineWith(_ => DownloadPermalink(permalinkId, cancellationToken))
                .OnSuccess(async tuple =>
                {
                    var (permalink, table) = tuple;
                    return await BuildViewModel(permalink, table);
                });
        }

        private async Task<Either<ActionResult, Permalink>> Find(Guid permalinkId,
            CancellationToken cancellationToken)
        {
            return await _contentDbContext.Permalinks
                .SingleOrNotFoundAsync(
                    predicate: permalink => permalink.Id == permalinkId,
                    cancellationToken: cancellationToken);
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
                .OnSuccess<ActionResult, TableBuilderResultViewModel, PermalinkViewModel>(async tableResult =>
                {
                    var frontendTableTask = _frontendService.CreateTable(
                        tableResult,
                        request.Configuration,
                        cancellationToken
                    );

                    var csvMetaTask = _permalinkCsvMetaService.GetCsvMeta(
                        request.Query.SubjectId,
                        tableResult.SubjectMeta,
                        cancellationToken
                    );

                    await Task.WhenAll(frontendTableTask, csvMetaTask);

                    var frontendTableResult = frontendTableTask.Result;
                    var csvMetaResult = csvMetaTask.Result;

                    if (frontendTableResult.IsLeft)
                    {
                        return frontendTableResult.Left;
                    }

                    if (csvMetaResult.IsLeft)
                    {
                        return csvMetaResult.Left;
                    }

                    var table = frontendTableResult.Right;
                    var csvMeta = csvMetaResult.Right;

                    var subjectMeta = tableResult.SubjectMeta;

                    // To avoid the frontend processing and returning the footnotes unnecessarily,
                    // create a new view model with the footnotes added directly
                    var tableWithFootnotes = table with
                    {
                        Footnotes = subjectMeta.Footnotes
                    };

                    var permalink = new Permalink
                    {
                        ReleaseId = releaseId,
                        SubjectId = request.Query.SubjectId,
                        PublicationTitle = subjectMeta.PublicationName,
                        DataSetTitle = subjectMeta.SubjectName,
                    };
                    _contentDbContext.Permalinks.Add(permalink);

                    await UploadSnapshot(permalink: permalink,
                        observations: tableResult.Results.ToList(),
                        csvMeta: csvMeta,
                        table: tableWithFootnotes,
                        cancellationToken: cancellationToken);

                    await _contentDbContext.SaveChangesAsync(cancellationToken);
                    return await BuildViewModel(permalink, tableWithFootnotes);
                });
        }

        public async Task<Either<ActionResult, Unit>> DownloadCsvToStream(Guid permalinkId,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            return await Find(permalinkId, cancellationToken)
                .OnSuccessVoid(() => _publicBlobStorageService.DownloadToStream(
                    containerName: BlobContainers.PermalinkSnapshots,
                    path: $"{permalinkId}.csv.zst",
                    stream: stream,
                    cancellationToken: cancellationToken
                ));
        }

        private static async Task WriteCsvHeaderRow(CsvWriter csv, PermalinkCsvMetaViewModel meta)
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
                row[header] = GetCsvRowValue(header,
                    observation,
                    timePeriod,
                    meta,
                    locationHeaders);
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

                if (location.TryGetValue(header, out var value))
                {
                    return value;
                }
            }

            if (meta.FiltersByGroupingColumn.TryGetValue(header, out var filterFoundFromGroupHeader))
            {
                var match = observation.Filters
                    .Where(filterItemId => filterFoundFromGroupHeader.Items.ContainsKey(filterItemId))
                    .OfType<Guid?>()
                    .FirstOrDefault(null as Guid?);

                if (match.HasValue)
                {
                    return filterFoundFromGroupHeader.Items[match.Value].GroupLabel;
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

        private async Task<PermalinkViewModel> BuildViewModel(Permalink permalink, PermalinkTableViewModel table)
        {
            var status = await GetPermalinkStatus(permalink.SubjectId);
            return new PermalinkViewModel
            {
                Id = permalink.Id,
                Created = permalink.Created,
                DataSetTitle = permalink.DataSetTitle,
                PublicationTitle = permalink.PublicationTitle,
                Status = status,
                Table = table
            };
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

        private async Task<Either<ActionResult, PermalinkTableViewModel>> DownloadPermalink(Guid permalinkId,
            CancellationToken cancellationToken = default)
        {
            return (await _publicBlobStorageService.GetDeserializedJson<PermalinkTableViewModel>(
                containerName: BlobContainers.PermalinkSnapshots,
                path: $"{permalinkId}.json.zst",
                cancellationToken: cancellationToken))!;
        }

        private async Task UploadSnapshot(Permalink permalink,
            List<ObservationViewModel> observations,
            PermalinkCsvMetaViewModel csvMeta,
            PermalinkTableViewModel table,
            CancellationToken cancellationToken = default)
        {
            await Task.WhenAll(
                UploadTableCsv(permalink, observations, csvMeta, cancellationToken),
                UploadTable(permalink, table, cancellationToken)
            );
        }

        private async Task UploadTableCsv(Permalink permalink,
            List<ObservationViewModel> observations,
            PermalinkCsvMetaViewModel csvMeta,
            CancellationToken cancellationToken = default)
        {
            await using var csvStream = new MemoryStream();
            await using var csvWriter =
                new CsvWriter(new StreamWriter(csvStream, leaveOpen: true), CultureInfo.InvariantCulture);
            await WriteCsvHeaderRow(csvWriter, csvMeta);
            await WriteCsvRows(csvWriter, observations, csvMeta, cancellationToken);
            await csvWriter.FlushAsync();

            await _publicBlobStorageService.UploadStream(
                containerName: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.csv.zst",
                stream: csvStream,
                contentType: ContentTypes.Csv,
                contentEncoding: ContentEncodings.Zstd,
                cancellationToken: cancellationToken
            );
        }

        private async Task UploadTable(Permalink permalink,
            PermalinkTableViewModel table,
            CancellationToken cancellationToken = default)
        {
            await _publicBlobStorageService.UploadAsJson(
                containerName: BlobContainers.PermalinkSnapshots,
                path: $"{permalink.Id}.json.zst",
                content: table,
                contentEncoding: ContentEncodings.Zstd,
                cancellationToken: cancellationToken
            );
        }
    }
}
