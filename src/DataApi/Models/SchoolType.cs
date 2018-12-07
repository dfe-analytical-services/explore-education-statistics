using System.ComponentModel;

namespace DataApi.Models
{
    public enum SchoolType
    {
        [Description("Total")] 
        Total,
        [Description("Primary")]
        Primary,
        [Description("Secondary")]
        Secondary,
        [Description("Special")]
        Special
    }
}