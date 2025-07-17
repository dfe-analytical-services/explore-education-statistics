using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class FunctionContextMockBuilder
{
    private string _functionName = "Mock Function Name";

    private class MockFunctionContext(FunctionDefinition functionDefinition) : FunctionContext
    {
        public override string InvocationId { get; } = string.Empty;
        public override string FunctionId { get; } = string.Empty;
        public override TraceContext TraceContext { get; } = null!;
        public override BindingContext BindingContext { get; } = null!;
        public override RetryContext RetryContext { get; } = null!;
        public override IServiceProvider InstanceServices { get; set; } = null!;
        public override FunctionDefinition FunctionDefinition => functionDefinition; 
        public override IDictionary<object, object> Items { get; set; } = null!;
        public override IInvocationFeatures Features { get; } = null!;
    }

    private class MockFunctionDefinition(string name) : FunctionDefinition
    {
        public override ImmutableArray<FunctionParameter> Parameters { get; } = ImmutableArray<FunctionParameter>.Empty;
        public override string PathToAssembly { get; } = string.Empty;
        public override string EntryPoint { get; } = string.Empty;
        public override string Id { get; } = string.Empty;
        public override string Name => name;
        public override IImmutableDictionary<string, BindingMetadata> InputBindings { get; } = ImmutableDictionary<string, BindingMetadata>.Empty;
        public override IImmutableDictionary<string, BindingMetadata> OutputBindings { get; } = ImmutableDictionary<string, BindingMetadata>.Empty;
    }

    public FunctionContext Build() => 
        new MockFunctionContext(
            new MockFunctionDefinition(
                name: _functionName));

    public FunctionContextMockBuilder ForFunctionName(string functionName)
    {
        _functionName = functionName;
        return this;
    }
}
