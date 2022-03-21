#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalAuthority : LocationAttribute, ILocationAttribute
    {
        public string? OldCode { get; }

        public LocalAuthority(string? code, string? oldCode, string? name) : base(code, name)
        {
            OldCode = oldCode;
        }

        public string GetCacheKey()
        {
            // Don't use GetCodeOrFallback here as the string needs to represent the local authority uniquely by all
            // attributes. Two local authorities with the same name and code but different old code are not identical.
            return $"{GetType().Name}:{Code ?? string.Empty}:{OldCode ?? string.Empty}:{Name ?? string.Empty}";
        }

        public string GetCodeOrFallback()
        {
            return string.IsNullOrEmpty(Code) ? OldCode ?? string.Empty : Code;
        }
    }
}
