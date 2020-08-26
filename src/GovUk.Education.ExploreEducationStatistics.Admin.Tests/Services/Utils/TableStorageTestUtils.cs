using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Cosmos.Table;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Utils
{
    public static class TableStorageTestUtils
    {
        public static Mock<CloudTable> MockCloudTable()
        {
            return new Mock<CloudTable>(
                new Uri("http://test.localhost/TestTable"),
                (TableClientConfiguration) null
            );
        }

        public static TableQuerySegment<TElement> CreateTableQuerySegment<TElement>(List<TElement> results)
        {
            var constructor = typeof(TableQuerySegment<TElement>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            Debug.Assert(constructor != null, nameof(constructor) + " != null");

            return constructor.Invoke(
                new object[] {results}
            ) as TableQuerySegment<TElement>;
        }
    }
}