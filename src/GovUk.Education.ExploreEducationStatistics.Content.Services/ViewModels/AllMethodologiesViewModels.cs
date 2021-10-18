#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public class AllMethodologiesThemeViewModel : IComparable<AllMethodologiesThemeViewModel>
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public List<AllMethodologiesTopicViewModel> Topics { get; set; } = new();

        public void RemoveTopicNodesWithoutMethodologiesAndSort()
        {
            // Remove all publications without any methodologies
            Topics.ForEach(topic => topic.RemovePublicationNodesWithoutMethodologiesAndSort());

            // Remove all topics without any publications
            Topics = Topics
                .Where(topic => topic.Publications.Any())
                .ToList();

            Topics.Sort();
        }

        public int CompareTo(AllMethodologiesThemeViewModel? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Title, other.Title, StringComparison.Ordinal);
        }
    }

    public class AllMethodologiesTopicViewModel : IComparable<AllMethodologiesTopicViewModel>
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public List<AllMethodologiesPublicationViewModel> Publications { get; set; } = new();

        public void RemovePublicationNodesWithoutMethodologiesAndSort()
        {
            Publications = Publications
                .Where(publication => publication.Methodologies.Any())
                .ToList();

            Publications.Sort();
        }


        public int CompareTo(AllMethodologiesTopicViewModel? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Title, other.Title, StringComparison.Ordinal);
        }
    }

    public class AllMethodologiesPublicationViewModel : IComparable<AllMethodologiesPublicationViewModel>
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public List<MethodologyVersionSummaryViewModel> Methodologies { get; set; } = new();


        public int CompareTo(AllMethodologiesPublicationViewModel? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Title, other.Title, StringComparison.Ordinal);
        }
    }
}
