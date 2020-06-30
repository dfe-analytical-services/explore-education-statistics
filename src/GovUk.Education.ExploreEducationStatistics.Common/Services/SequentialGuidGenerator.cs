using System;
using System.Threading;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    /**
     * Generates sequential <see cref="Guid" /> values using the same algorithm as NEWSEQUENTIALID() in Microsoft SQL Server.
     * Copy of <see cref="Microsoft.EntityFrameworkCore.ValueGeneration.SequentialGuidValueGenerator" /> without the ValueGenerator interface.
     * Used to reduce fragmentation of clustered indexes on Guid fields.
     * See https://edgamat.com/2020/01/19/Sequential-Guid-Values-For-SQL-Server.html.
     */
    public class SequentialGuidGenerator : IGuidGenerator
    {
        private long _counter = DateTime.UtcNow.Ticks;

        public Guid NewGuid()
        {
            var guidBytes = Guid.NewGuid().ToByteArray();
            var counterBytes = BitConverter.GetBytes(Interlocked.Increment(ref _counter));

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(counterBytes);
            }

            guidBytes[08] = counterBytes[1];
            guidBytes[09] = counterBytes[0];
            guidBytes[10] = counterBytes[7];
            guidBytes[11] = counterBytes[6];
            guidBytes[12] = counterBytes[5];
            guidBytes[13] = counterBytes[4];
            guidBytes[14] = counterBytes[3];
            guidBytes[15] = counterBytes[2];

            return new Guid(guidBytes);
        }
    }
}