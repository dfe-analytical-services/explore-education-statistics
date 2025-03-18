using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using Azure;
using Azure.Core;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services.EventGrid;

public class MockResponse : Response
{
    public override void Dispose() => throw new NotImplementedException();

    protected override bool TryGetHeader(string name, [NotNullWhen(true)] out string? value) => throw new NotImplementedException();

    protected override bool TryGetHeaderValues(string name, [NotNullWhen(true)] out IEnumerable<string>? values) => throw new NotImplementedException();

    protected override bool ContainsHeader(string name) => throw new NotImplementedException();

    protected override IEnumerable<HttpHeader> EnumerateHeaders() => throw new NotImplementedException();

    public override bool IsError => this.StatusCode != HttpStatusCode.OK;

    public override int Status => (int)StatusCode;
    public override string ReasonPhrase { get; } = string.Empty;
    public override Stream? ContentStream { get; set; }
    public override string ClientRequestId { get; set; } = string.Empty;
    public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.OK;
}
