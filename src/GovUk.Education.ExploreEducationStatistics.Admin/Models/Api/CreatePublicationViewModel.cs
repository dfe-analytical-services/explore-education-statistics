using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CreatePublicationViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public Guid TopicId { get; set; }

        public Guid? MethodologyId { get; set; }

        [Required]
        public Guid ContactId { get; set; }
    }
}