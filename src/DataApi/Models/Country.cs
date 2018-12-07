namespace DataApi.Models
{
    public class Country
    {
        public Country(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public string Name { get; set; }
        
        private string Code { get; set; }
    }
}