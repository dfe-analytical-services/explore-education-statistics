#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    // TODO EES-2462 Consider making this a class with Name and Id properties
    public enum Role
    {
        [EnumLabelValue("Analyst", "f9ddb43e-aa9e-41ed-837d-3062e130c425")]
        Analyst,
        [EnumLabelValue("BAU User", "cf67b697-bddd-41bd-86e0-11b7e11d99b3")]
        BauUser,
        [EnumLabelValue("Prerelease User", "17e634f4-7a2b-4a23-8636-b079877b4232")]
        PrereleaseUser
    }
}
