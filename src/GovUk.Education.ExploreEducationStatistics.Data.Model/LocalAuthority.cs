using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalAuthority : ObservationalUnit
    {
        [JsonIgnore] public string OldCode { get; }

        public LocalAuthority()
        {
        }

        public LocalAuthority(string code, string oldCode, string name) : base(code, name)
        {
            OldCode = oldCode;
        }

        public string GetCodeOrOldCodeIfEmpty()
        {
            return Code.IsNullOrEmpty() ? OldCode : Code;
        }
    }
}