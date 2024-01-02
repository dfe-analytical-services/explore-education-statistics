using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Cosmos.Table;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public static class TableStorageTestUtils
{
    public static Mock<CloudTable> MockCloudTable(string tableName = "table-name")
    {
        return new Mock<CloudTable>(
            new Uri($"http://127.0.0.1:10002/devstoreaccount1/{tableName}"),
            null!);
    }

    public static TableQuerySegment<TElement> CreateTableQuerySegment<TElement>(
        List<TElement> results, TableContinuationToken continuationToken = null)
    {
        // https://github.com/Azure/azure-storage-net/issues/619#issuecomment-364090291
        var constructor = typeof(TableQuerySegment<TElement>)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(c => c.GetParameters().Count() == 1);

        var tableQuerySegment = constructor!.Invoke(
            new object[] {results}
        ) as TableQuerySegment<TElement>;

        var continuationTokenField = typeof(TableQuerySegment<TElement>)
            .GetField("continuationToken",
                BindingFlags.Instance | BindingFlags.SetField | BindingFlags.NonPublic);

        continuationTokenField!.SetValue(tableQuerySegment, continuationToken);

        return tableQuerySegment;
    }
}
