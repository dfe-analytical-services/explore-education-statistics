namespace DataApi.Models
{
    public class LocalAuthority
    {
        public LocalAuthority(string name, string code, string oldCode)
        {
            Name = name;
            Code = code;
            Old_Code = oldCode;
        }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Old_Code { get; set; }
    }
}