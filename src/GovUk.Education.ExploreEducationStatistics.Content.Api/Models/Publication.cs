using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Models
{
    public class Publication
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Slug { get; set; }

        public Guid TopicId { get; set; }
        public Topic Topic { get; set; }
    }
}