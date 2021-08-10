#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class CacheKeyParamAttribute : Attribute
    {
        /// <summary>
        /// The specific name of the parameter in the
        /// caching key constructor to use.
        /// </summary>
        public string? Name { get; set; }

        public CacheKeyParamAttribute(string? name = null)
        {
            Name = name;
        }
    }
}