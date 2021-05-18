using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class UserViewModel
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public string Email { get; set; }
        
        public string Role { get; set; }

        public List<UserPublicationRoleViewModel> UserPublicationRoles { get; set; }
        
        public List<UserReleaseRoleViewModel> UserReleaseRoles { get; set; }
    }

    public class UserPublicationRoleViewModel
    {
        public Guid Id { get; set; }
        public string Publication { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PublicationRole Role { get; set; }
    }
    
    public class UserReleaseRoleViewModel
    {
        public Guid Id { get; set; }
        public string Publication { get; set; }
        public string Release { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseRole Role { get; set; }
    }
}
