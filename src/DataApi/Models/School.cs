namespace DataApi.Models
{
    public class School
    {
        public School()
        {
        }

        public School(int estab, int laestab, string acadType, string acadOpend)
        {
            this.estab = estab;
            this.laestab = laestab;
            acad_type = acadType;
            acad_opend = acadOpend;
        }

        public int estab { get; set; }
        
        public int laestab { get; set; }
        
        public string acad_type { get; set; }
        
        public string acad_opend { get; set; }
    }
}