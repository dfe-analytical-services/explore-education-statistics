using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public interface IVersioned<T> where T : IVersion
    {
        List<T> Versions { get; set; }

        T Latest { get; }

        T Current { get; }

    }
}