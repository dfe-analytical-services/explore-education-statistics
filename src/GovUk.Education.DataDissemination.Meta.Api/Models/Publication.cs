using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GovUk.Education.DataDissemination.Meta.Api.Models
{
    public class Publication
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }
        
        public int TopicId { get; set; }
        public Topic Topic { get; set; }  
    }
}