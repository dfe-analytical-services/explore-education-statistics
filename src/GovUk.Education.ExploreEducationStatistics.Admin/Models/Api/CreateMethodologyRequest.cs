using System;
using System.ComponentModel.DataAnnotations;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CreateMethodologyRequest
    {
        [Required] public string Title { get; set; }

        public DateTime? PublishScheduled { get; set; }
        
        // TODO EES-899 Contact in the request is being ignored!
        [Required] public Guid ContactId { get; set; }

        public string Slug => SlugFromTitle(Title);
    }
}