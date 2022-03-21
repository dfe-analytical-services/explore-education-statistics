using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class User
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string DisplayName => $"{FirstName} {LastName}";
    }
}
