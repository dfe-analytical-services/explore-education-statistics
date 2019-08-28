using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class AbstractVersioned<T> : IVersioned<T> where T : IVersion
    {
        public List<T> Versions { get; set; }

        [NotMapped] public List<T> Ordered => Versions?.OrderBy(t => t.Created).ToList() ?? new List<T>();

        [NotMapped] public T Latest => Ordered.LastOrDefault();
        
        [NotMapped] public T First => Ordered.FirstOrDefault();

        [NotMapped] public T Current => ForDate(DateTime.Now);
        
        private T ForDate(DateTime dateTime)
        {
            return Ordered.LastOrDefault(t => t.Created < dateTime);
        }
    }
}