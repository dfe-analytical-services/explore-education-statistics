namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public Guid? DeletedById { get; set; }

    public User DeletedBy { get; set; }

    public DateTime? SoftDeleted { get; set; }

    public string DisplayName => $"{FirstName} {LastName}";
}
