#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static System.String;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record PublicationSaveRequest
{
        [Required] public string Title { get; set; } = Empty;

        [Required, MaxLength(160)] public string Summary { get; set; } = Empty;

        [Required] public Guid TopicId { get; set; }

        // @MarkFix if you update a publication title, it doesn't even attempt to update slug
        // and maybe the methodology slug should change now?
        private string _slug = Empty;

        public string Slug
        {
            get => IsNullOrEmpty(_slug) ? NamingUtils.SlugFromTitle(Title) : _slug;
            set => _slug = value;
        }

        public Guid? SupersededById { get; set; }

}
