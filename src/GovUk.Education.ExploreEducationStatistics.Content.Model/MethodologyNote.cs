#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyNote
    {
        public Guid Id { get; set; }

        public string Content { get; set; } = null!;

        public DateTime DisplayDate { get; set; }

        public Guid MethodologyVersionId { get; set; }

        public MethodologyVersion MethodologyVersion { get; set; } = null!;

        public DateTime Created { get; set; }

        public User CreatedBy { get; set; } = null!;

        public Guid CreatedById { get; set; }

        public DateTime? Updated { get; set; }

        public Guid? UpdatedById { get; set; }

        public User UpdatedBy { get; set; } = null!;

        public MethodologyNote Clone(MethodologyVersion newVersion)
        {
            var copy = (MethodologyNote) MemberwiseClone();

            copy.Id = Guid.NewGuid();
            copy.MethodologyVersion = newVersion;
            copy.MethodologyVersionId = newVersion.Id;

            return copy;
        }
    }
}
