using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Security.DataSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ResultSubjectMetaService : AbstractSubjectMetaService, IResultSubjectMetaService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IBoundaryLevelRepository _boundaryLevelRepository;
        private readonly IFootnoteRepository _footnoteRepository;
        private readonly IIndicatorRepository _indicatorRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IUserService _userService;
        private readonly ISubjectRepository _subjectRepository;
        private readonly ILogger<ResultSubjectMetaService> _logger;
        private readonly IMapper _mapper;
        private readonly IReleaseDataFileRepository _releaseDataFileRepository;

        public ResultSubjectMetaService(ContentDbContext contentDbContext,
            IBoundaryLevelRepository boundaryLevelRepository,
            IFilterItemRepository filterItemRepository,
            IFootnoteRepository footnoteRepository,
            IGeoJsonRepository geoJsonRepository,
            IIndicatorRepository indicatorRepository,
            ILocationRepository locationRepository,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper,
            ITimePeriodService timePeriodService,
            IUserService userService,
            ISubjectRepository subjectRepository,
            ILogger<ResultSubjectMetaService> logger,
            IMapper mapper,
            IReleaseDataFileRepository releaseDataFileRepository) : base(boundaryLevelRepository, filterItemRepository, geoJsonRepository)
        {
            _contentDbContext = contentDbContext;
            _boundaryLevelRepository = boundaryLevelRepository;
            _footnoteRepository = footnoteRepository;
            _indicatorRepository = indicatorRepository;
            _locationRepository = locationRepository;
            _persistenceHelper = persistenceHelper;
            _timePeriodService = timePeriodService;
            _userService = userService;
            _subjectRepository = subjectRepository;
            _logger = logger;
            _mapper = mapper;
            _releaseDataFileRepository = releaseDataFileRepository;
        }

        public Task<Either<ActionResult, ResultSubjectMetaViewModel>> GetSubjectMeta(Guid releaseId, 
            SubjectMetaQueryContext query, IQueryable<Observation> observations)
        {
            return _persistenceHelper.CheckEntityExists<Subject>(query.SubjectId)
                .OnSuccess(CheckCanViewSubjectData)
                .OnSuccess(async subject =>
                {
                    var filters = _logger.TraceTime(() => 
                            GetFilters(query.SubjectId, observations, true),
                        "Getting Filters");

                    var indicators = _logger.TraceTime(
                        () => GetIndicators(query),
                        "Getting Indicators");

                    var observationalUnits = _logger.TraceTime(
                        () => _locationRepository.GetObservationalUnits(observations),
                        "Getting Observational Units");

                    var timePeriodRange = _logger.TraceTime(
                        () => GetTimePeriodRange(observations),
                        "Getting Time Periods");

                    var footnotes = _logger.TraceTime(
                        () => GetFilteredFootnotes(releaseId, observations, query),
                        "Getting Footnotes");

                    var geoJsonAvailable = _logger.TraceTime(
                        () => HasBoundaryLevelDataForAnyObservationalUnits(observationalUnits),
                        "Getting GeoJsonAvailable");

                    var boundaryLevels = _logger.TraceTime(
                        () => GetBoundaryLevelOptions(query.BoundaryLevel, observationalUnits.Keys),
                        "Getting BoundaryLevels");

                    var locations = _logger.TraceTime(() => 
                        GetGeoJsonObservationalUnits(observationalUnits, query.IncludeGeoJson ?? false, query.BoundaryLevel),
                        "Getting GeoJson Observational Units");

                    var publicationId = await _subjectRepository.GetPublicationIdForSubject(subject.Id);
                    var publicationTitle = (await _contentDbContext.Publications.FindAsync(publicationId)).Title;

                    var releaseFile = await _releaseDataFileRepository.GetBySubject(releaseId, subject.Id);
                    var subjectName = releaseFile.Name;
                    
                    return new ResultSubjectMetaViewModel
                    {
                        Filters = filters,
                        Footnotes = footnotes,
                        GeoJsonAvailable = geoJsonAvailable,
                        Indicators = indicators,
                        Locations = locations,
                        BoundaryLevels = boundaryLevels,
                        PublicationName = publicationTitle,
                        SubjectName = subjectName,
                        TimePeriodRange = timePeriodRange
                    };
                });
        }

        private async Task<Either<ActionResult, Subject>> CheckCanViewSubjectData(Subject subject)
        {
            if (await _userService.MatchesPolicy(subject, CanViewSubjectData))
            {
                return subject;
            }

            return new ForbidResult();
        }

        private IEnumerable<ObservationalUnitMetaViewModel> GetGeoJsonObservationalUnits(
            Dictionary<GeographicLevel, IEnumerable<ObservationalUnit>> observationalUnits,
            bool geoJsonRequested,
            long? boundaryLevelId)
        {
            var viewModels = observationalUnits.SelectMany(pair =>
                BuildObservationalUnitMetaViewModelsWithGeoJsonIfAvailable(
                    pair.Key,
                    pair.Value.ToList(),
                    geoJsonRequested,
                    boundaryLevelId));

            return TransformDuplicateObservationalUnitsWithUniqueLabels(viewModels)
                .OrderBy(model => model.Level.ToString())
                .ThenBy(model => model.Label);
        }

        private IEnumerable<BoundaryLevelIdLabel> GetBoundaryLevelOptions(long? boundaryLevelId,
            IEnumerable<GeographicLevel> geographicLevels)
        {
            var boundaryLevels = boundaryLevelId.HasValue
                ? _boundaryLevelRepository.FindRelatedByBoundaryLevel(boundaryLevelId.Value)
                : _boundaryLevelRepository.FindByGeographicLevels(geographicLevels);
            return boundaryLevels.Select(level => _mapper.Map<BoundaryLevelIdLabel>(level));
        }

        private IEnumerable<IndicatorMetaViewModel> GetIndicators(SubjectMetaQueryContext query)
        {
            var indicators = _indicatorRepository.GetIndicators(query.SubjectId, query.Indicators);
            return BuildIndicatorViewModels(indicators);
        }

        private IEnumerable<FootnoteViewModel> GetFilteredFootnotes(Guid releaseId, IQueryable<Observation> observations,
            SubjectMetaQueryContext queryContext)
        {
            return _footnoteRepository.GetFilteredFootnotes(releaseId, queryContext.SubjectId, observations, queryContext.Indicators)
                .Select(footnote => new FootnoteViewModel
                {
                    Id = footnote.Id,
                    Label = footnote.Content
                });
        }

        private IEnumerable<TimePeriodMetaViewModel> GetTimePeriodRange(IQueryable<Observation> observations)
        {
            return _timePeriodService.GetTimePeriodRange(observations).Select(tuple =>
                new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));
        }
    }
}
