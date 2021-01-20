using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class File
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public Release Release { get; set; }

        public Guid ReleaseId { get; set; }

        public Guid? SubjectId { get; set; }

        public string Filename { get; set; }

        public FileType Type { get; set; }

        public string BlobStorageName
        {
            get
            {
                // Ideally all blob storage files should be
                // saved by ID instead of file name to
                // prevent naming collisions.
                // Remove this BlobStorageName field and use Id when all types are migrated
                if (Type == Ancillary || Type == Chart)
                {
                    return Id.ToString();
                }

                return Filename;
            }
        }

        public Guid? ReplacedById { get; set; }
        public File ReplacedBy { get; set; }

        public Guid? ReplacingId { get; set; }
        public File Replacing { get; set; }

        public Guid? SourceId { get; set; }
        public File? Source { get; set; }

        // EES-1560 Temporary field used to track the filename migration to guids.
        public bool FilenameMigrated { get; set; }
    }
}
