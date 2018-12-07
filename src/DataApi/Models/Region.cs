namespace DataApi.Models
{
    public class Region
    {
        public Region(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public string Name { get; set; }
        
        private string Code { get; set; }
    }
}