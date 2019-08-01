using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class EditReleaseSummaryViewModel
    {
        
        public Guid Id { get; set; }
        
        [Required]
        public Guid? TypeId { get; set; }
        
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        [Required]
        public TimeIdentifier? TimePeriodCoverage { get; set; }
        
        public DateTime? PublishScheduled { get; set; }
        
        [PartialDateValidator]
        public PartialDate NextReleaseDate { get; set; }
     
        [RegularExpression(@"^([0-9]{4})?$")]
        public string ReleaseName { get; set; }
    }
}