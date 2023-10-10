#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationSummaryViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;
        
        public bool Owner { get; set; }
        
        public ContactViewModel Contact { get; set; }
        
        public PublicationSummaryViewModel()
        {
        }

        public PublicationSummaryViewModel(Guid id, string title, string slug, bool owner, Contact contact)
        {
            Id = id;
            Title = title;
            Slug = slug;
            Owner = owner;
            Contact = new ContactViewModel(contact);
        }

        public PublicationSummaryViewModel(PublicationCacheViewModel publicationCache)
        {
            Id = publicationCache.Id;
            Title = publicationCache.Title;
            Slug = publicationCache.Slug;
            Contact = publicationCache.Contact;
        }
    }
}
