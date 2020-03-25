using System;
using System.ComponentModel.DataAnnotations;
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CreateMethodologyViewModel
    {
        [Required]
        public string Title { get; set; }

        public DateTime? PublishScheduled { get; set; }

        [Required]
        public Guid ContactId { get; set; }
        
        public Guid? PublicationId { get; set; }
        
        public string Slug => SlugFromTitle(Title);
    }
}