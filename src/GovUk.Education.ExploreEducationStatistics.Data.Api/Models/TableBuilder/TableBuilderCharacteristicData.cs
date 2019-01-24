using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderCharacteristicData : ITableBuilderData
    {
        public int Year { get; set; }
        public string SchoolType { get; set; }
        public CharacteristicViewModel Characteristic { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}