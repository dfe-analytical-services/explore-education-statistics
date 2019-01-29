using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ModelBinding
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CommaSeparatedQueryStringAttribute : Attribute
    {
    }
}