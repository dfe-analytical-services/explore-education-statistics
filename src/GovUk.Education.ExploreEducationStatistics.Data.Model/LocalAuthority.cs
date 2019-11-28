using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class LocalAuthority : IObservationalUnit
    {
        public string Code { get; set; }
        [JsonIgnore] public string OldCode { get; set; }
        public string Name { get; set; }

        public LocalAuthority()
        {
        }

        public LocalAuthority(string code, string oldCode, string name)
        {
            Code = code;
            OldCode = oldCode;
            Name = name;
        }

        public static LocalAuthority Empty()
        {
            return new LocalAuthority(null, null, null);
        }

        public string GetCodeOrOldCodeIfEmpty()
        {
            return string.IsNullOrEmpty(Code) ? OldCode : Code;
        }

        protected bool Equals(LocalAuthority other)
        {
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LocalAuthority) obj);
        }

        public override int GetHashCode()
        {
            return (Code != null ? Code.GetHashCode() : 0);
        }
    }
}