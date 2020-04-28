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
    }
}