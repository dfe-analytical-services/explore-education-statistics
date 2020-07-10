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
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class PublicationMetaService : IPublicationMetaService
    {
        private readonly IReleaseService _releaseService;
        private readonly ISubjectService _subjectService;
        private readonly ITableStorageService _tableStorageService;
        private readonly IMapper _mapper;

        public PublicationMetaService(IReleaseService releaseService,
            ISubjectService subjectService,
            ITableStorageService tableStorageService,
            IMapper mapper)
        {
            _releaseService = releaseService;
            _subjectService = subjectService;
            _tableStorageService = tableStorageService;
            _mapper = mapper;
        }

        public async Task<Either<ActionResult, PublicationSubjectsMetaViewModel>> GetSubjectsForLatestRelease(
            Guid publicationId)
        {
            var releaseId = _releaseService.GetLatestPublishedRelease(publicationId);
            if (!releaseId.HasValue)
            {
                return new NotFoundResult();
            }

            return new PublicationSubjectsMetaViewModel
            {
                PublicationId = publicationId,
                Highlights = await GetHighlights(releaseId.Value),
                Subjects = await GetSubjects(releaseId.Value)
            };
        }

        private async Task<IEnumerable<IdLabel>> GetHighlights(Guid releaseId)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ReleaseFastTrack.PartitionKey),
                QueryComparisons.Equal, releaseId.ToString());
            var query = new TableQuery<ReleaseFastTrack>().Where(filter);

            return (await _tableStorageService.ExecuteQueryAsync(PublicReleaseFastTrackTableName, query))
                .Where(releaseFastTrack => !string.IsNullOrEmpty(releaseFastTrack.HighlightName))
                .Select(releaseFastTrack => new IdLabel(releaseFastTrack.FastTrackId, releaseFastTrack.HighlightName));
        }

        private async Task<IEnumerable<IdLabel>> GetSubjects(Guid releaseId)
        {
            var subjects = await _subjectService.GetSubjectsForReleaseAsync(releaseId);
            return _mapper.Map<IEnumerable<IdLabel>>(subjects);
        }
    }
}