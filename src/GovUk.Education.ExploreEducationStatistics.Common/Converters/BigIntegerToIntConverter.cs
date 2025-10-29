using System.Numerics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters;

public class BigIntegerToIntConverter()
    : ValueConverter<BigInteger, int>(bigInt => (int)bigInt, intVal => new BigInteger(intVal)) { }
