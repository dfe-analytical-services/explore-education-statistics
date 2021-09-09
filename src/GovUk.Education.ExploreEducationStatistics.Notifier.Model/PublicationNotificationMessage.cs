using System;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model
{
    public class PublicationNotificationMessage
    {
        public Guid PublicationId { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }

        protected bool Equals(PublicationNotificationMessage other)
        {
            return Name == other.Name && PublicationId.Equals(other.PublicationId) && Slug == other.Slug;
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
            return HashCode.Combine(Name, PublicationId, Slug);
        }
    }
}
