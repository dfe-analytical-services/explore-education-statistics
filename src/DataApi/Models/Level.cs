using System.ComponentModel;

namespace DataApi.Models
{
    public enum Level
    {
        [Description("National")] 
        National,
        
        [Description("Region")]
        Region,
        
        [Description("Local authority")]
        LocalAuthority,
        
        [Description("School")]
        School
    }
}