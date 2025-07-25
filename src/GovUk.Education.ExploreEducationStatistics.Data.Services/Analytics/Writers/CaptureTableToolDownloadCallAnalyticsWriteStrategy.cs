﻿#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Strategies;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Analytics.Writers;

public class CaptureTableToolDownloadCallAnalyticsWriteStrategy(
    IAnalyticsPathResolver analyticsPathResolver,
    ICommonAnalyticsWriteStrategyWorkflow<CaptureTableToolDownloadCall> workflow) : IAnalyticsWriteStrategy
{
    public static readonly string[] OutputSubPaths = ["public", "table-tool-downloads", "table-tool-page"];

    private readonly IWorkflowActor<CaptureTableToolDownloadCall> _workflowActor =
        new WorkflowActor(analyticsPath: analyticsPathResolver.BuildOutputDirectory(OutputSubPaths));
    
    public Type RequestType => typeof(CaptureTableToolDownloadCall);

    public async Task Report(IAnalyticsCaptureRequest request, CancellationToken cancellationToken = default)
    {
        if (request is not CaptureTableToolDownloadCall captureRequest)
        {
            throw new ArgumentException(
                $"Request must be of type {nameof(CaptureTableToolDownloadCall)}. It is {request.GetType().FullName}", 
                nameof(request));
        }
        await workflow.Report(_workflowActor, captureRequest, cancellationToken);   
    }
    
    private class WorkflowActor(string analyticsPath)
        : WorkflowActorBase<CaptureTableToolDownloadCall>(analyticsPath)
    {
        public override string GetFilenamePart(CaptureTableToolDownloadCall request) =>
            $"{request.ReleaseVersionId}-{request.SubjectId}";
    }
}
