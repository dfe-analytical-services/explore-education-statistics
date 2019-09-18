using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class SubjectService : AbstractRepository<Subject, long>, ISubjectService
    {
        private readonly IReleaseService _releaseService;

        public SubjectService(StatisticsDbContext context,
            ILogger<SubjectService> logger, IReleaseService releaseService) : base(context, logger)
        {
            _releaseService = releaseService;
        }

        public bool IsSubjectForLatestRelease(long subjectId)
        {
            var subject = Find(subjectId, new List<Expression<Func<Subject, object>>> {subject1 => subject1.Release});
            if (subject == null)
            {
                throw new ArgumentException("Subject does not exist", nameof(subjectId));
            }
            return _releaseService.GetLatestRelease(subject.Release.PublicationId).Equals(subject.ReleaseId);
        }
        
        public bool Exists(ReleaseId id, string name)
        {
            return _context.Subject.Any(x => x.ReleaseId == id && x.Name == name);
        }
    }
}