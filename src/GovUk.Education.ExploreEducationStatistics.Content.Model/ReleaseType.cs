using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseType
    {
        public Guid Id { get; set; }

        [Required] public string Title { get; set; }

        protected bool Equals(ReleaseType other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ReleaseType) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}