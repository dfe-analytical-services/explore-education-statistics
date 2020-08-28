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

        public string BlobStorageName
        {
            get
            {
                // Ideally all blob storage files should be
                // saved by ID instead of file name to
                // prevent naming collisions.
                if (ReleaseFileType == ReleaseFileTypes.Chart)
                {
                    return Id.ToString();
                }

                return Filename;
            }
        }
        public Guid? SourceId { get; set; }
        public ReleaseFileReference? Source { get; set; }
    }
}