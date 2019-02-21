using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DataFileAttribute : Attribute
    {
        public Type EntityType { get; }

        public DataFileAttribute(Type entityType)
        {
            EntityType = entityType;
        }
    }
}