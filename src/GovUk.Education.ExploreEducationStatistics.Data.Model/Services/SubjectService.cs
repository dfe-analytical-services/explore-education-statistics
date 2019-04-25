using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SubjectService : AbstractDataService<Subject, long>, ISubjectService
    {
        private readonly IReleaseService _releaseService;

        public SubjectService(ApplicationDbContext context,
            ILogger<SubjectService> logger, IReleaseService releaseService) : base(context, logger)
        {
            _releaseService = releaseService;
        }

        public bool IsSubjectForLatestRelease(long subjectId)
        {
            var subject = Find(subjectId, new List<Expression<Func<Subject, object>>> {subject1 => subject1.Release});
            return _releaseService.GetLatestRelease(subject.Release.PublicationId) == subject.ReleaseId;
        }
        
//
//        public Dictionary<string, IEnumerable<CharacteristicMeta>> GetCharacteristicMetaGroups(long subjectId)
//        {
//            return DbSet().Where(s => s.Id == subjectId)
//                .SelectMany(s => s.Characteristics)
//                .GroupBy(characteristicMeta => characteristicMeta.Group)
//                .ToDictionary(
//                    metas => metas.Key,
//                    metas => metas.AsEnumerable());
//        }
    }
}