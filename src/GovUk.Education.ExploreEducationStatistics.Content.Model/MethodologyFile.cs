#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyFile
    {
        public Guid Id { get; set; }

        public MethodologyVersion MethodologyVersion { get; set; } = null!;

        public Guid MethodologyVersionId { get; set; }

        public File File { get; set; } = null!;

        public Guid FileId { get; set; }
    }
}
