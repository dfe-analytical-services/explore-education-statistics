namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    /// <summary>
    /// Class that can be used as a return type representing no meaningful value. 
    /// </summary>
    public sealed class Unit
    {
        private Unit()
        {
        }

        public static readonly Unit Instance = new Unit();
    }
}