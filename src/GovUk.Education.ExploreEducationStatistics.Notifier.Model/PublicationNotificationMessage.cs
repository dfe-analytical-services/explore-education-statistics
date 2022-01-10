#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model
{
    public class PublicationNotificationMessage
    {
        public Guid PublicationId { get; set; }
        public string PublicationName { get; set; } = "";
        public string PublicationSlug { get; set; } = "";

        public string ReleaseName { get; set; } = "";
        public string ReleaseSlug { get; set; } = "";

        public bool Amendment { get; set; }
        public string UpdateNote { get; set; } = "";

        protected bool Equals(PublicationNotificationMessage other)
        {
            return PublicationId.Equals(other.PublicationId)
                   && PublicationName == other.PublicationName
                   && PublicationSlug == other.PublicationSlug
                   && ReleaseName == other.ReleaseName
                   && ReleaseSlug == other.ReleaseSlug;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PublicationNotificationMessage) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PublicationId, PublicationName, PublicationSlug, ReleaseName, ReleaseSlug, Amendment);
        }
    }
}
