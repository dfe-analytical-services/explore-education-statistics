using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public static class SchoolTypes
    {
        private static readonly Dictionary<SchoolType, string> values = new Dictionary<SchoolType, string>
        {
            {SchoolType.Total, "Total"},
            {SchoolType.State_Funded_Primary, "State-funded primary"},
            {SchoolType.State_Funded_Secondary, "State-funded secondary"},
            {SchoolType.Special, "Special"}
        };

        public static string getStringFromEnum(SchoolType value)
        {
            return values.GetValueOrDefault(value);
        }

        public static SchoolType getEnumFromString(string value)
        {
            foreach (var keyValuePair in values)
            {
                if (keyValuePair.Value == value)
                {
                    return keyValuePair.Key;
                }
            }

            throw new ArgumentException("Unexpected value: " + value);
        }
    }
}