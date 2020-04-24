using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class ReleaseSummaryViewModel
    {
        
        public Guid Id { get; set; }
        
        [Required]
        public ReleaseType Type { get; set; }
        
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        [Required]
        public TimeIdentifier TimePeriodCoverage { get; set; }
        
        public DateTime? PublishScheduled { get; set; }
        
        [PartialDateValidator]
        public PartialDate NextReleaseDate { get; set; }
     
        [RegularExpression(@"^([0-9]{4})?$")]
        public string ReleaseName { get; set; }
        
        public string YearTitle { get; set; }
        
        private string _slug;
        public string Slug
        {
            get => IsNullOrEmpty(_slug) ? SlugFromTitle(Format(ReleaseName, TimePeriodCoverage)) : _slug;
            set => _slug = value; 
        }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseStatus Status { get; set; }

        public string InternalReleaseNote { get; set; }
    }
}