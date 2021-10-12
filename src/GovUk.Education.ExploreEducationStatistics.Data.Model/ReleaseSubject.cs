using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseSubject
    {
        public Subject Subject { get; set; }

        public Guid SubjectId { get; set; }

        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public string DataGuidance { get; set; }

        public ReleaseSubject CopyForRelease(Release release)
        {
            var releaseSubject = MemberwiseClone() as ReleaseSubject;

            releaseSubject.Release = release;
            releaseSubject.ReleaseId = release.Id;

            return releaseSubject;
        }
    }
}