using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class PublicationViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public List<ReleaseViewModel> Releases { get; set; }

        public List<LegacyReleaseViewModel> LegacyReleases { get; set; }

        public MethodologyTitleViewModel Methodology { get; set; }
        
        public ExternalMethodology ExternalMethodology { get; set; }

        public Guid TopicId { get; set; }

        public Guid ThemeId { get; set; }

        public ContactViewModel Contact { get; set; }
    }

    public class SavePublicationViewModel
    {
        [Required] public string Title { get; set; }

        [Required] public Guid TopicId { get; set; }

        public Guid? MethodologyId { get; set; }

        public ExternalMethodology ExternalMethodology { get; set; }

        [Required] public SaveContactViewModel Contact { get; set; }

        private string _slug;

        public string Slug
        {
            get => IsNullOrEmpty(_slug) ? SlugFromTitle(Title) : _slug;
            set => _slug = value;
        }
    }
}