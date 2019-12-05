using System;
using System.ComponentModel.DataAnnotations;
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api
{
    public class CreateTopicRequest
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }
        
        private string _slug;
        
        public string Slug
        {
            get => IsNullOrEmpty(_slug) ? SlugFromTitle(Title) : _slug;
            set => _slug = value; 
        }
    }
}