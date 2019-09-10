using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityEnumPropertyConverterAttribute : Attribute
    {
        public EntityEnumPropertyConverterAttribute()
        {
        }
    }
}