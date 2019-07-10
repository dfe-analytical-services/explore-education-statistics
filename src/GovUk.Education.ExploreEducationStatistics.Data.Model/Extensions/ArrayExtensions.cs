using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions
{
    public static class ArrayExtensions
    {
        public static T[,] ToMultidimensionalArray<T>(this IEnumerable<T[]> enumerable)
        {
            var jaggedArray = enumerable.ToArray();
            return jaggedArray.ToMultidimensionalArray();
        }
        
        private static T[,] ToMultidimensionalArray<T>(this T[][] jaggedArray)
        {
            var rows = jaggedArray.Length;
            var cols = rows == 0 ? 0 : jaggedArray.Max(subArray => subArray.Length);
            var array = new T[rows, cols];
            for (var i = 0; i < rows; i++)
            {
                cols = jaggedArray[i].Length;
                for (var j = 0; j < cols; j++)
                {
                    array[i, j] = jaggedArray[i][j];
                }
            }

            return array;
        }
    }
}