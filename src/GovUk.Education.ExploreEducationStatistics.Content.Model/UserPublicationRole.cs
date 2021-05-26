using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class UserPublicationRole
    {
        public Guid Id { get; set; }

        public User User { get; set; }

        public Guid UserId { get; set; }

        public Publication Publication { get; set; }

        public Guid PublicationId { get; set; }

        public PublicationRole Role { get; set; }

        public DateTime Created { get; set; }

        public User CreatedBy { get; set; }

        public Guid CreatedById { get; set; }
    }
}
