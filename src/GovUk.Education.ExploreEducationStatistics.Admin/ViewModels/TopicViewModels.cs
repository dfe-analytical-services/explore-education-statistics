using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class TopicViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public Guid ThemeId { get; set; }
    }

    public class TopicSaveViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public Guid ThemeId { get; set; }

        private string _slug;

        public string Slug
        {
            get => String.IsNullOrEmpty(_slug) ? NamingUtils.SlugFromTitle(Title) : _slug;
            set => _slug = value;
        }
    }
}