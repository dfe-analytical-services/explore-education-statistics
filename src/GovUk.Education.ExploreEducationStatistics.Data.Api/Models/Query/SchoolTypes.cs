using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query
{
    public static class SchoolTypes
    {
        private static readonly Dictionary<SchoolType, string[]> values = new Dictionary<SchoolType, string[]>
        {
            {SchoolType.Dummy, new [] {"Dummy"}},
            {SchoolType.Total, new [] {"Total"}},
            {SchoolType.State_Funded_Primary, new [] {"State-funded primary"}},
            {SchoolType.State_Funded_Secondary, new [] {"State-funded secondary"}},
            {SchoolType.Special, new [] {"Special"}}
        };

        public static string EnumToString(SchoolType value)
        {
            return value.ToString();
        }
        
        public static SchoolType EnumFromStringForImport(string value)
        {
            foreach (var keyValuePair in values)
            {
                if (keyValuePair.Value.Select(s => s.ToLower()).Contains(value.ToLower()))
                {
                    return keyValuePair.Key;
                }
            }

            throw new ArgumentException("Unexpected value: " + value);
        }
    }
}