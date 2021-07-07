using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class MyMethodologyViewModel
    {
        public Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MethodologyStatus Status { get; set; }
        
        public bool Amendment { get; set; }
        
        public Guid PreviousVersionId { get; set; }

        public DateTime? Published { get; set; }

        public string? InternalReleaseNote { get; set; }

        public PermissionsSet Permissions { get; set; }

        public string Title { get; set; }

        public class PermissionsSet
        {
            public bool CanUpdateMethodology { get; set; }
            public bool CanDeleteMethodology { get; set; }
            public bool CanMakeAmendmentOfMethodology { get; set; }
        }
    }
}
