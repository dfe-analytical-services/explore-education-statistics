// @MarkFix nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class PublicationViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public string Slug { get; set; }

        public Guid TopicId { get; set; }

        public Guid ThemeId { get; set; }

        public ContactViewModel Contact { get; set; }

        public Guid? SupersededById { get; set; }

        public bool IsSuperseded { get; set; }

        public PublicationPermissions? Permissions { get; set; }

        public record PublicationPermissions
        {
            public bool CanUpdatePublication { get; set; }
            public bool CanUpdatePublicationTitle { get; set; }
            public bool CanUpdatePublicationSupersededBy { get; set; }
            public bool CanCreateReleases { get; set; }
            public bool CanAdoptMethodologies { get; set; }
            public bool CanCreateMethodologies { get; set; }
            public bool CanManageExternalMethodology { get; set; }
        }
    }

    public class PublicationSaveViewModel
    {
        [Required] public string Title { get; set; }

        [Required, MaxLength(160)] public string Summary { get; set; }

        [Required] public Guid TopicId { get; set; }

        public ExternalMethodology ExternalMethodology { get; set; }

        [Required] public ContactSaveViewModel Contact { get; set; }

        private string _slug;

        public string Slug
        {
            get => IsNullOrEmpty(_slug) ? SlugFromTitle(Title) : _slug;
            set => _slug = value;
        }

        public Guid? SupersededById { get; set; }
    }
}
