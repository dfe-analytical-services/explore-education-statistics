using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database
{   
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumLabelValueAttribute : Attribute
    {
        public string Label { get; }
        public string Value { get; }

        public EnumLabelValueAttribute(string label, string value)
        {
            Label = label;
            Value = value;
        }
        
        public EnumLabelValueAttribute(string label)
        {
            Label = label;
        }
    }
}