using System;

#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels
{
    public record PublicationTreeNode : BasePublicationTreeNode
    {
        public string Summary { get; init; } = string.Empty;

        public string? LegacyPublicationUrl { get; init; }

        public virtual bool Equals(PublicationTreeNode? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Summary == other.Summary && LegacyPublicationUrl == other.LegacyPublicationUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Summary, LegacyPublicationUrl);
        }
    }
}