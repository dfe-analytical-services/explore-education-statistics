using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserPublicationRoleCreateRequest
    {
        public Guid PublicationId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PublicationRole PublicationRole { get; set; }
    }
}
