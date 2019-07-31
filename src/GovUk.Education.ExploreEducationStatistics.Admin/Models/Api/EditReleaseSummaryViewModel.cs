using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class EditReleaseViewModel
    {
        
        public Guid Id { get; set; }
        
        [Required]
        public Guid? ReleaseTypeId { get; set; }
        
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        [Required]
        public TimeIdentifier? TimePeriodCoverage { get; set; }
        
        public DateTime? PublishScheduled { get; set; }
        
        [PartialDateValidator]
        public PartialDate NextReleaseExpected { get; set; }
     
        [RegularExpression(@"^([0-9]{4})?$")]
        public string ReleaseName { get; set; }
    }
}