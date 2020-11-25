using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseFileReference
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public Guid? SubjectId { get; set; }

        public string Filename { get; set; }

        public ReleaseFileTypes ReleaseFileType { get; set; }

        public Guid? ReplacedById { get; set; }
        public ReleaseFileReference ReplacedBy { get; set; }

        public Guid? ReplacingId { get; set; }
        public ReleaseFileReference Replacing { get; set; }

        public Guid? SourceId { get; set; }
        public ReleaseFileReference? Source { get; set; }

        // EES-1560 Temporary field used to track the filename migration to guids.
        public bool FilenameMigrated { get; set; }
    }
}