using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta
{
    public interface IMeta
    {
        long Id { get; set; }
        string Name { get; set; }
        string Label { get; set; }
    }
}