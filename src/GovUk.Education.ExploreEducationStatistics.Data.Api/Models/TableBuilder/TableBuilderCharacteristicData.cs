using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder
{
    public class TableBuilderCharacteristicData : ITableBuilderData
    {
        public int Year { get; set; }
        public SchoolType SchoolType { get; set; }
        public CharacteristicViewModel Characteristic { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }
}