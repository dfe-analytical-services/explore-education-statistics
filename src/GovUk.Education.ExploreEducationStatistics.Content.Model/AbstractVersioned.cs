using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class AbstractVersioned<T> : IVersioned<T> where T : IVersion
    {
        

        public List<T> Versions { get; set; }

        public List<T> Ordered => Versions?.OrderBy(t => t.Created).ToList() ?? new List<T>();

        public T Lastest => Ordered.LastOrDefault();

        public T Current => ForDate(DateTime.Now);

        public T ForDate(DateTime dateTime)
        {
            return Ordered.FirstOrDefault(t => t.Created < dateTime);
        }
    }
}