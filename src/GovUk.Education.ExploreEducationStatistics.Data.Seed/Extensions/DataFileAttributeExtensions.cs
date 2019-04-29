using System;
using System.Linq;
using System.Reflection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed.Extensions
{
    public static class DataFileAttributeExtensions
    {
        public static DataCsvMetaFilename GetMetaFilename(this Enum enumValue)
        {
            return typeof(DataCsvFilename).GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DataFileAttribute>().MetaFilename;
        }
    }
}