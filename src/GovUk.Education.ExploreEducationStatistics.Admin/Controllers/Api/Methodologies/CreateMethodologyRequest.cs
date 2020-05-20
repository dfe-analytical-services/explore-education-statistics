using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies
{
    public class CreateMethodologyRequest
    {
        [Required] public string Title { get; set; }

        public DateTime? PublishScheduled { get; set; }
        
        // TODO EES-899 Contact in the request is being ignored!
        [Required] public Guid ContactId { get; set; }
    }
}