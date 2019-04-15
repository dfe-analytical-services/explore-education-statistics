using System;
using System.Linq;
using System.Reflection;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    public static class DataFileAttributeExtensions
    {
        public static DataCsvMetaFilename GetMetaFilenameFromDataFileAttributeOfEnumType(this Enum enumValue,
            Type enumType)
        {
            return enumType.GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DataFileAttribute>().MetaFilename;
        }
    }
}