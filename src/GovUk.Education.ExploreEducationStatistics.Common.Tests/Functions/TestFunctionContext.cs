using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;

public class TestFunctionContext : FunctionContext
{
    public override string InvocationId => string.Empty;

    public override string FunctionId => string.Empty;

    public override TraceContext TraceContext { get; }

    public override BindingContext BindingContext { get; }

    public override RetryContext RetryContext { get; }

    public override IServiceProvider InstanceServices { get; set; }

    public override FunctionDefinition FunctionDefinition { get; } = new TestFunctionDefinition();

    public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();

    public override IInvocationFeatures Features { get; }
}
