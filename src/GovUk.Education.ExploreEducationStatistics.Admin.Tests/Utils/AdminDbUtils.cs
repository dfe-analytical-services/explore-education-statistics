using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils;

public static class AdminDbUtils
{
    public static WebApplicationFactory<TEntrypoint> AddUsersAndRolesDbTestData<TEntrypoint>(
        this WebApplicationFactory<TEntrypoint> app,
        Action<UsersAndRolesDbContext> testData
    )
        where TEntrypoint : class
    {
        return app.AddTestData(testData);
    }

    public static WebApplicationFactory<TEntrypoint> VerifyUsersAndRolesDbContext<TEntrypoint>(
        this WebApplicationFactory<TEntrypoint> app,
        Action<UsersAndRolesDbContext> verificationAction)
        where TEntrypoint : class
    {
        var context = (app.Services.GetService(typeof(UsersAndRolesDbContext)) as UsersAndRolesDbContext)!;
        verificationAction(context);
        return app;
    }

    public static WebApplicationFactory<TEntrypoint> ResetUsersAndRolesDbContext<TEntrypoint>(
        this WebApplicationFactory<TEntrypoint> app)
        where TEntrypoint : class
    {
        return app.ResetDbContext<UsersAndRolesDbContext, TEntrypoint>();
    }
}
