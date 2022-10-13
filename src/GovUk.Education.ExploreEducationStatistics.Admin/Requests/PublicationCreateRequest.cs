#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static System.String;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record PublicationCreateRequest
{
        [Required] public string Title { get; set; } = Empty;

        [Required, MaxLength(160)] public string Summary { get; set; } = Empty;

        [Required] public Guid TopicId { get; set; }

        [Required] public ContactSaveViewModel Contact { get; set; } = null!;

        private string _slug = Empty;

        public string Slug
        {
            get => IsNullOrEmpty(_slug) ? NamingUtils.SlugFromTitle(Title) : _slug;
            set => _slug = value;
        }

        public Guid? SupersededById { get; set; }

}
