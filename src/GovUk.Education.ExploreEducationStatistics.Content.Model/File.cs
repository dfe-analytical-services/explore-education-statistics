#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class File : ICreatedTimestamp<DateTime?>
    {
        public Guid Id { get; set; }

        // TODO EES-3552 Remove nullable and migrate db column to be not-null
        public string? ContentType { get; set; }

        public Guid RootPath { get; set; }

        public Guid? SubjectId { get; set; }

        public string Filename { get; set; } = string.Empty;

        public FileType Type { get; set; }

        public Guid? ReplacedById { get; set; }

        public File? ReplacedBy { get; set; }

        public Guid? ReplacingId { get; set; }

        public File? Replacing { get; set; }

        public Guid? SourceId { get; set; }

        public File? Source { get; set; }

        // TODO EES-3552 Remove nullable and migrate db column to be not-null
        public long? Size { get; set; }

        public DateTime? Created { get; set; }

        public User? CreatedBy { get; set; }

        public Guid? CreatedById { get; set; }
    }
}
