#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public record UserViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string? Role { get; set; }

        public List<UserPublicationRoleViewModel> UserPublicationRoles { get; set; } = new();
        
        public List<UserReleaseRoleViewModel> UserReleaseRoles { get; set; } = new();
    }

    public record UserPublicationRoleViewModel
    {
        public Guid Id { get; set; }
        
        public string Publication { get; set; } = string.Empty;
        
        [JsonConverter(typeof(StringEnumConverter))]
        public PublicationRole Role { get; set; }
        
        public string UserName { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
    }
    
    public record UserReleaseRoleViewModel
    {
        public Guid Id { get; set; }
        
        public string Publication { get; set; } = null!;
        
        public string Release { get; set; } = null!;
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseRole Role { get; set; }
    }
}
