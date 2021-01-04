using System;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseFile
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public Release Release { get; set; }
        
        public Guid ReleaseId { get; set; }

        public File File { get; set; }

        public Guid FileId { get; set; }

        public ReleaseFile CreateReleaseAmendment(Release amendment)
        {
            var copy = MemberwiseClone() as ReleaseFile;

            copy.Id = Guid.NewGuid();
            copy.Release = amendment;
            copy.ReleaseId = amendment.Id;

            return copy;
        }
    }
}