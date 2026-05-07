#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests.UserManagement;

public record UserPublicationRoleCreateRequest
{
    public Guid PublicationId { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public PublicationRole PublicationRole { get; init; }

    public class Validator : AbstractValidator<UserPublicationRoleCreateRequest>
    {
        public Validator()
        {
            RuleFor(x => x.PublicationId).NotEmpty();
        }
    }
}
