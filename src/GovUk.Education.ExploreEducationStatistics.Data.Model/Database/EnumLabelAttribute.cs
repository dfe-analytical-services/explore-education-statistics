using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{   
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumLabelAttribute : Attribute
    {
        public string Label { get; }

        public EnumLabelAttribute(string label)
        {
            Label = label;
        }
    }
}