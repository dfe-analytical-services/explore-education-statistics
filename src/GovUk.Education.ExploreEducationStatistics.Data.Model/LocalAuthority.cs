#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalAuthority : ObservationalUnit, ILocationAttribute
    {
        public string? OldCode { get; }

        public LocalAuthority(string? code, string? oldCode, string? name) : base(code, name)
        {
            OldCode = oldCode;
        }

        public string GetCodeOrFallback()
        {
            return GetCodeOrOldCodeIfEmpty() ?? string.Empty;
        }

        public string? GetCodeOrOldCodeIfEmpty()
        {
            return Code.IsNullOrEmpty() ? OldCode : Code;
        }

        public static LocalAuthority Empty()
        {
            return new(null, null, null);
        }
    }
}
