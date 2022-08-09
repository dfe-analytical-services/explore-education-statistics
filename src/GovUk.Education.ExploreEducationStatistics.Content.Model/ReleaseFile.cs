#nullable enable
using System;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseFile
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public Release Release { get; set; } = null!;

        public Guid ReleaseId { get; set; }

        public File File { get; set; } = null!;

        public Guid FileId { get; set; }

        public string? Name { get; set; }

        public string? Summary { get; set; }

        public int Order { get; set; }

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
