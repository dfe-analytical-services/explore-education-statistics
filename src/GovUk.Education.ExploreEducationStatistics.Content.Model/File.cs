using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class File
    {
        public Guid Id { get; set; }

        public Guid RootPath { get; set; }

        public Guid? SubjectId { get; set; }

        public string Filename { get; set; }

        public FileType Type { get; set; }

        public Guid? ReplacedById { get; set; }

        public File ReplacedBy { get; set; }

        public Guid? ReplacingId { get; set; }

        public File Replacing { get; set; }

        public Guid? SourceId { get; set; }

        public File? Source { get; set; }

        public DateTime? Created { get; set; }

        public User CreatedBy { get; set; }

        public Guid? CreatedById { get; set; }

        // EES-1704 Temporary fields that will be removed after files migration
        public bool PrivateBlobPathMigrated { get; set; }
        public bool PublicBlobPathMigrated { get; set; }
    }
}
