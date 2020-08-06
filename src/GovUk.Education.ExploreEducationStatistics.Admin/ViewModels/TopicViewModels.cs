using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class TopicViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string Description { get; set; }

        public Guid ThemeId { get; set; }

        public string Summary { get; set; }

        public List<MyPublicationViewModel> Publications { get; set; }
    }

    public class SaveTopicViewModel
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }

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