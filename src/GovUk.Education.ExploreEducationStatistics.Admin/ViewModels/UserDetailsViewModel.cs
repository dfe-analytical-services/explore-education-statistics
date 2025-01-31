#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record UserDetailsViewModel
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; }

    public string Email { get; set; }

    public UserDetailsViewModel(Guid id, string displayName, string email)
    {
        Id = id;
        DisplayName = displayName;
        Email = email;
    }

    public UserDetailsViewModel(User user)
    {
        Id = user.Id;
        DisplayName = user.DisplayName;
        Email = user.Email;
    }
}