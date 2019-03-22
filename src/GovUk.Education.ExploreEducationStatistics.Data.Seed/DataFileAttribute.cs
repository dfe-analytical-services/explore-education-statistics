using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DataFileAttribute : Attribute
    {
        public Type DataType { get; }

        public DataFileAttribute(Type dataType)
        {
            DataType = dataType;
        }
    }
}