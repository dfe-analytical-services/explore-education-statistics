using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CreateReleaseViewModel
    {
        public Guid PublicationId { get; set; }

        [Required]
        public Guid ReleaseTypeId { get; set; }
        
        [Required]
        [JsonConverter(typeof(TimeIdentifierJsonConverter))]
        public TimeIdentifier TimePeriodCoverage { get; set; }
        
        public DateTime? PublishScheduled { get; set; }
        
        [PartialDateValidator]
        public PartialDate NextReleaseDate { get; set; }
     
        [RegularExpression(@"^([0-9]{4})?$")]
        public string ReleaseName { get; set; }

        public Guid? TemplateReleaseId { get; set; }
        
        private string _slug;
        public string Slug
        {
            get => IsNullOrEmpty(_slug) ? SlugFromTitle(ReleaseTitle(ReleaseName, TimePeriodCoverage)) : _slug;
            set => _slug = value; 
        }
    }
}