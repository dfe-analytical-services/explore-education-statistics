#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels
{
    public class GlossaryEntryViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        protected bool Equals(GlossaryEntryViewModel other)
        {
            return Title == other.Title && Slug == other.Slug && Body == other.Body;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GlossaryEntryViewModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title, Slug, Body);
        }
    }

    public class GlossaryCategoryViewModel
    {
        public string Heading { get; set; } = string.Empty;

        public List<GlossaryEntryViewModel> Entries { get; set; }

        protected bool Equals(GlossaryCategoryViewModel other)
        {
            return Heading == other.Heading && Entries.SequenceEqual(other.Entries);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GlossaryCategoryViewModel)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Heading, Entries);
        }
    }
}
