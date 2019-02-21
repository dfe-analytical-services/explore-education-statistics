using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DataFileAttribute : Attribute
    {
        public Type Type { get; }

        public DataFileAttribute(Type type)
        {
            Type = type;
        }
    }
}