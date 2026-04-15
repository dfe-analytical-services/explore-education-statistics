#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record UserViewModel
{
    public required Guid Id { get; init; }

    public required string Name { get; init; } = string.Empty;

    public required string Email { get; init; } = string.Empty;

    public string? Role { get; init; }

    public List<UserPublicationRoleViewModel> UserPublicationRoles { get; init; } = [];

    public List<UserPrereleaseRoleViewModel> UserPrereleaseRoles { get; init; } = [];
}

public record UserPublicationRoleViewModel
{
    public required Guid Id { get; init; }

    public required string Publication { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required PublicationRole Role { get; init; }
}

public record UserPublicationRoleWithUserViewModel : UserPublicationRoleViewModel
{
    public required string UserName { get; init; }

    public required string Email { get; init; }
}

public record UserPrereleaseRoleViewModel
{
    public required Guid Id { get; init; }

    public required string Publication { get; init; }

    public required string Release { get; init; }
}
