using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly IReleaseService _releaseService;
        private readonly ISubjectService _subjectService;
        private readonly ITableStorageService _tableStorageService;
        private readonly IMapper _mapper;

        public PublicationService(
            IReleaseService releaseService,
            ISubjectService subjectService,
            ITableStorageService tableStorageService,
            IMapper mapper)
        {
            _releaseService = releaseService;
            _subjectService = subjectService;
            _tableStorageService = tableStorageService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, PublicationViewModel>> GetPublication(
            Guid publicationId)
        {
            var release = _releaseService.GetLatestPublishedRelease(publicationId);

            if (release == null)
            {
                return new NotFoundResult();
            }

            return new PublicationViewModel
            {
                Id = publicationId,
                Highlights = await GetHighlights(release.Id),
                Subjects = await GetSubjects(release.Id)
            };
        }

        private async Task<IEnumerable<IdLabel>> GetHighlights(Guid releaseId)
        {
            var releaseFilter = TableQuery.GenerateFilterCondition(nameof(ReleaseFastTrack.PartitionKey),
                QueryComparisons.Equal, releaseId.ToString());

            var highlightFilter = TableQuery.GenerateFilterCondition(nameof(ReleaseFastTrack.HighlightName), QueryComparisons.NotEqual,
                    string.Empty);

            var combineFilter = TableQuery.CombineFilters(releaseFilter, TableOperators.And, highlightFilter);
            var query = new TableQuery<ReleaseFastTrack>().Where(combineFilter);

            return (await _tableStorageService.ExecuteQueryAsync(PublicReleaseFastTrackTableName, query))
                .Select(releaseFastTrack => new IdLabel(releaseFastTrack.FastTrackId, releaseFastTrack.HighlightName));
        }

        private async Task<IEnumerable<IdLabel>> GetSubjects(Guid releaseId)
        {
            var subjects = await _subjectService.GetSubjectsForRelease(releaseId);
            return _mapper.Map<IEnumerable<IdLabel>>(subjects);
        }
    }
}