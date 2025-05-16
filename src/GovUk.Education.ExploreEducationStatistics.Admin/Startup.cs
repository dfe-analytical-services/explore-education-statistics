#nullable enable
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Filters;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Options;
using GovUk.Education.ExploreEducationStatistics.Events.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Interfaces;
using Semver;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Repositories;
using Thinktecture;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using ContentGlossaryService = GovUk.Education.ExploreEducationStatistics.Content.Services.GlossaryService;
using ContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.MethodologyService;
using ContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.PublicationService;
using ContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.ReleaseService;
using DataGuidanceService = GovUk.Education.ExploreEducationStatistics.Admin.Services.DataGuidanceService;
using DataSetService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data.DataSetService;
using GlossaryService = GovUk.Education.ExploreEducationStatistics.Admin.Services.GlossaryService;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;
using IContentGlossaryService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IGlossaryService;
using IContentMethodologyService =
    GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IMethodologyService;
using IContentPublicationService =
    GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using IContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;
using IDataGuidanceService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IDataGuidanceService;
using IDataSetService =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data.IDataSetService;
using IGlossaryService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IGlossaryService;
using IMethodologyImageService =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies.IMethodologyImageService;
using IMethodologyService =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies.IMethodologyService;
using IPublicationRepository =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationService;
using IReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseFileService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using IReleaseVersionRepository =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using IReleaseVersionService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionService;
using IThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IThemeService;
using MethodologyImageService =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies.MethodologyImageService;
using MethodologyService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies.MethodologyService;
using PublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationRepository;
using PublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;
using ReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseFileService;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseService;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionRepository;
using ReleaseVersionService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionService;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;
using ThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ThemeService;

namespace GovUk.Education.ExploreEducationStatistics.Admin
{
    public class Startup(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            // TODO EES-5073 Remove this when the Public Data db exists in ALL Azure environments.
            var publicDataDbExists = configuration.GetValue<bool>("PublicDataDbExists");

            services.AddHealthChecks();

            /*
             * Logging
             */

            services.AddApplicationInsightsTelemetry()
                .AddApplicationInsightsTelemetryProcessor<SensitiveDataTelemetryProcessor>();

            /*
             * Web configuration
             */

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = _ => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });

            services.AddControllers(options =>
                {
                    options.AddCommaSeparatedQueryModelBinderProvider();
                    options.AddTrimStringBinderProvider();
                })
                .AddControllersAsServices();

            services.AddHttpContextAccessor();

            services.AddFluentValidation();

            services.AddValidatorsFromAssemblies([
                typeof(UploadDataSetRequest.Validator).Assembly, // Adds *all* validators from Admin
                typeof(FullTableQueryRequest.Validator).Assembly // Adds *all* validators from Common
            ]);

            services.AddMvc(options =>
                {
                    options.Filters.Add(new AuthorizeFilter(SecurityPolicies.RegisteredUser.ToString()));
                    options.Filters.Add(new OperationCancelledExceptionFilter());
                    options.Filters.Add(new ProblemDetailsResultFilter());
                    options.EnableEndpointRouting = false;
                    options.AllowEmptyInputInBodyModelBinding = true;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            // Adds Brotli and Gzip compressing
            services.AddResponseCompression(options => { options.EnableForHttps = true; });

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(config => { config.RootPath = "wwwroot"; });
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            /*
             * Database contexts
             */

            // TODO EES-4869 - review if we need to retain these tables.
            services.AddDbContext<UsersAndRolesDbContext>(options =>
                options
                    .UseSqlServer(configuration.GetConnectionString("ContentDb"),
                        providerOptions =>
                            providerOptions
                                .MigrationsAssembly(typeof(Startup).Assembly.FullName)
                                .EnableCustomRetryOnFailure()
                    )
                    .EnableSensitiveDataLogging(hostEnvironment.IsDevelopment())
            );

            services.AddDbContext<ContentDbContext>(options =>
                options
                    .UseSqlServer(configuration.GetConnectionString("ContentDb"),
                        providerOptions =>
                            providerOptions
                                .MigrationsAssembly(typeof(Startup).Assembly.FullName)
                                .EnableCustomRetryOnFailure()
                    )
                    .EnableSensitiveDataLogging(hostEnvironment.IsDevelopment())
            );

            services.AddDbContext<StatisticsDbContext>(options =>
                options
                    .UseSqlServer(configuration.GetConnectionString("StatisticsDb"),
                        providerOptions =>
                            providerOptions
                                .MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model")
                                .AddBulkOperationSupport()
                                .EnableCustomRetryOnFailure()
                    )
                    .EnableSensitiveDataLogging(hostEnvironment.IsDevelopment())
            );

            // Only set up the `PublicDataDbContext` in non-integration test
            // environments. Otherwise, the connection string will be null and
            // cause the data source builder to throw a host exception.
            if (!hostEnvironment.IsIntegrationTest())
            {
                // TODO EES-5073 Remove this check when the Public Data db is available in all Azure environments.
                if (publicDataDbExists)
                {
                    var publicDataDbConnectionString = configuration.GetConnectionString("PublicDataDb")!;
                    services.AddPsqlDbContext<PublicDataDbContext>(publicDataDbConnectionString, hostEnvironment);
                }
            }

            /*
             * Authentication and Authorization
             */

            //
            // This configuration sets up out-of-the-box Services to support the Identity Framework's Users, Roles and
            // Claims. It sets up the UserManager and RoleManager in DI, and links them to the database via the
            // UsersAndRolesDbContext.
            //
            // AddIdentityCore() sets up the bare minimum configuration to provide JWT validation and setting of the
            // ClaimsPrincipal on the HttpContext based upon the content of incoming JWTs, as opposed to AddIdentity()
            // which sets up additional configuration such as Forbidden and Login routes, which we don't need.
            //
            // In order to provide it with strategies for how to deal with invalid JWTs, unauthenticated users etc
            // though, we need to register a handler for these.
            //
            // TODO EES-4869 - review if we want to keep these features of Identity Framework or not.
            services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Stores.MaxLengthForKeys = 128;
                })
                .AddRoles<IdentityRole>()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddUserStore<UserStore<ApplicationUser, IdentityRole, UsersAndRolesDbContext>>()
                .AddRoleStore<RoleStore<IdentityRole, UsersAndRolesDbContext>>()
                .AddEntityFrameworkStores<UsersAndRolesDbContext>();

            services.Configure<IdentityOptions>(options =>
            {
                // Allow special characters such as apostrophes and @ symbols to be permitted in AspNetUsers'
                // "Username" column.  This allows us to store email addresses as Usernames when newly invited users
                // sign in.
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+'";
            });
            // This service helps to add additional information to the ClaimsPrincipal on the HttpContext after
            // Identity Framework has verified that the incoming JWTs are valid (and has created the basic
            // ClaimsPrincipal already from information in the JWT).
            services.AddTransient<IClaimsTransformation, ClaimsPrincipalTransformationService>();

            if (!hostEnvironment.IsIntegrationTest())
            {
                services
                    // This tells Identity Framework to look for Bearer tokens in incoming requests' Authorization
                    // headers as a way of identifying users.
                    .AddAuthentication(options =>
                    {
                        // This line tells Identity Framework to use the JWT mechanism for verifying users based upon
                        // JWTs found in Bearer tokens carried in the Authorization headers of requests made to the
                        // Admin API.  It also tells Identity Framework to use its default AuthenticationHandler
                        // implementation for handling challenges, forbid errors etc with appropriate status code
                        // responses rather than redirect responses.
                        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    // This adds verification of the incoming JWTs after they have been located in the
                    // Authorization headers above.
                    .AddMicrosoftIdentityWebApi(configuration.GetRequiredSection("OpenIdConnectIdentityFramework"));

                // This helps Identity Framework with incoming websocket requests from SignalR, or any request where
                // adding the Bearer token in the Authorization HTTP header is not possible, and is instead added as an
                // "access_token" query parameter. This code grabs the token from the query parameter and makes it
                // available for Identity Framework to find (in order for it to validate it and build its
                // ClaimsPrincipal).
                services.Configure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        var originalOnMessageReceived = options.Events.OnMessageReceived;

                        options.Events.OnMessageReceived = async context =>
                        {
                            await originalOnMessageReceived(context);

                            if (!context.Token.IsNullOrEmpty())
                            {
                                return;
                            }

                            if (context.Request.Query.ContainsKey("access_token"))
                            {
                                context.Token = context.Request.Query["access_token"];
                            }
                        };
                    });
            }

            /*
             * SignalR
             */

            var signalRBuilder = services
                .AddSignalR(
                    options =>
                    {
                        options.AddFilter<HttpContextHubFilter>();
                    }
                )
                .AddNewtonsoftJsonProtocol();

            var azureSignalRConnectionString = configuration.GetValue<string>("Azure:SignalR:ConnectionString");

            if (!azureSignalRConnectionString.IsNullOrEmpty())
            {
                signalRBuilder.AddAzureSignalR(azureSignalRConnectionString);
            }

            /*
             * Configuration options
             */

            services.Configure<AppOptions>(
                configuration.GetRequiredSection(AppOptions.Section));
            services.Configure<AppInsightsOptions>(
                configuration.GetSection(AppInsightsOptions.Section));
            services.Configure<NotifyOptions>(
                configuration.GetSection(NotifyOptions.Section));
            services.Configure<PublicAppOptions>(
                configuration.GetRequiredSection(PublicAppOptions.Section));
            services.Configure<PublicDataProcessorOptions>(
                configuration.GetRequiredSection(PublicDataProcessorOptions.Section));
            services.Configure<PublicDataApiOptions>(
                configuration.GetRequiredSection(PublicDataApiOptions.Section));
            services.Configure<PreReleaseAccessOptions>(
                configuration.GetRequiredSection(PreReleaseAccessOptions.Section));
            services.Configure<LocationsOptions>(
                configuration.GetRequiredSection(LocationsOptions.Section));
            services.Configure<ReleaseApprovalOptions>(
                configuration.GetRequiredSection(ReleaseApprovalOptions.Section));
            services.Configure<TableBuilderOptions>(
                configuration.GetRequiredSection(TableBuilderOptions.Section));
            services.Configure<OpenIdConnectSpaClientOptions>(
                configuration.GetSection(OpenIdConnectSpaClientOptions.Section));
            services.Configure<FeatureFlags>(
                configuration.GetSection(FeatureFlags.Section));
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            StartupSecurityConfiguration.ConfigureAuthorizationPolicies(services);

            /*
             * Services
             */
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // This service is responsible for handling calls immediately following successful login into the Admin SPA.
            // It will determine if the user is an existing or a new user, and will register them locally if a new user.
            services.AddTransient<ISignInService, SignInService>();

            // TODO EES-3510 These services from the Content.Services namespace are used to update cached resources.
            // EES-3528 plans to send a request to the Content API to update its cached resources instead of this
            // being done from Admin directly, and so these DI dependencies should eventually be removed.
            services.AddTransient<IContentGlossaryService, ContentGlossaryService>();
            services.AddTransient<IContentMethodologyService, ContentMethodologyService>();
            services.AddTransient<IContentPublicationService, ContentPublicationService>();
            services.AddTransient<IContentReleaseService, ContentReleaseService>();
            services.AddTransient<IGlossaryCacheService, GlossaryCacheService>();
            services.AddTransient<IMethodologyCacheService, MethodologyCacheService>();
            services.AddTransient<IPublicationCacheService, PublicationCacheService>();
            services.AddTransient<IPublicationCacheService, PublicationCacheService>();
            services.AddTransient<IReleaseCacheService, ReleaseCacheService>();

            services.AddTransient<IFileRepository, FileRepository>();
            services.AddTransient<IDataImportRepository, DataImportRepository>();
            services.AddTransient<IReleaseFileRepository, ReleaseFileRepository>();
            services.AddTransient<IReleaseDataFileRepository, ReleaseDataFileRepository>();

            services.AddTransient<IReleaseDataFileService, ReleaseDataFileService>();
            services.AddTransient<IDataSetFileStorage, DataSetFileStorage>();
            services.AddTransient<IDataGuidanceFileWriter, DataGuidanceFileWriter>();
            services.AddTransient<IReleaseFileService, ReleaseFileService>();
            services.AddTransient<IReleaseImageService, ReleaseImageService>();
            services.AddTransient<IReleasePermissionService, ReleasePermissionService>();
            services.AddTransient<IDataImportService, DataImportService>();
            services.AddTransient<IImportStatusBauService, ImportStatusBauService>();
            services.AddTransient<IPublishingService, PublishingService>();
            services.AddTransient<IReleasePublishingStatusService, ReleasePublishingStatusService>();
            services.AddTransient<IReleasePublishingStatusRepository, ReleasePublishingStatusRepository>();
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<IPublicationService, PublicationService>();
            services.AddTransient<IPublicationRepository, PublicationRepository>();
            services.AddTransient<IMetaService, MetaService>();
            services.AddTransient<IReleaseSlugValidator, ReleaseSlugValidator>();
            services.AddTransient<IReleaseVersionService, ReleaseVersionService>();
            services.AddTransient<IReleaseService, ReleaseService>();
            services.AddTransient<IReleaseAmendmentService, ReleaseAmendmentService>();
            services.AddTransient<IReleaseApprovalService, ReleaseApprovalService>();
            services.AddTransient<ReleaseSubjectRepository.SubjectDeleter, ReleaseSubjectRepository.SubjectDeleter>();
            services.AddTransient<IReleaseSubjectRepository, ReleaseSubjectRepository>();
            services.AddTransient<IReleaseChecklistService, ReleaseChecklistService>();
            services.AddTransient<IReleaseVersionRepository, ReleaseVersionRepository>();
            services.AddTransient<IMethodologyService, MethodologyService>();
            services.AddTransient<IMethodologyNoteService, MethodologyNoteService>();
            services.AddTransient<IMethodologyNoteRepository, MethodologyNoteRepository>();
            services.AddTransient<IMethodologyVersionRepository, MethodologyVersionRepository>();
            services.AddTransient<IMethodologyRepository, MethodologyRepository>();
            services.AddTransient<IMethodologyContentService, MethodologyContentService>();
            services.AddTransient<IMethodologyFileRepository, MethodologyFileRepository>();
            services.AddTransient<IMethodologyImageService, MethodologyImageService>();
            services.AddTransient<IMethodologyAmendmentService, MethodologyAmendmentService>();
            services.AddTransient<IMethodologyApprovalService, MethodologyApprovalService>();
            services.AddTransient<IDataBlockService, DataBlockService>();
            services.AddTransient<IPreReleaseUserService, PreReleaseUserService>();
            services.AddTransient<IPreReleaseService, PreReleaseService>();
            services.AddTransient<IPreReleaseSummaryService, PreReleaseSummaryService>();

            services.AddTransient<IManageContentPageService, ManageContentPageService>();
            services.AddTransient<IContentBlockService, ContentBlockService>();
            services.AddTransient<IContentService, ContentService>();
            services.AddTransient<IEmbedBlockService, EmbedBlockService>();
            services.AddTransient<IContentBlockLockService, ContentBlockLockService>();
            services.AddTransient<IKeyStatisticService, KeyStatisticService>();
            services.AddTransient<IFeaturedTableService, FeaturedTableService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<IRelatedInformationService, RelatedInformationService>();
            services.AddTransient<IReplacementService, ReplacementService>();
            services.AddTransient<IUserRoleService, UserRoleService>();
            services.AddTransient<IUserReleaseRoleService, UserReleaseRoleService>();
            services.AddTransient<IUserPublicationRoleRepository, UserPublicationRoleRepository>();
            services.AddTransient<IUserReleaseRoleRepository, UserReleaseRoleRepository>();
            services.AddTransient<IUserReleaseInviteRepository, UserReleaseInviteRepository>();
            services.AddTransient<IUserPublicationInviteRepository, UserPublicationInviteRepository>();
            services.AddTransient<IRedirectsCacheService, RedirectsCacheService>();
            services.AddTransient<IRedirectsService, RedirectsService>();
            services.AddTransient<IDataSetCandidateService, DataSetCandidateService>();
            services.AddTransient<IPostgreSqlRepository, PostgreSqlRepository>();
            services.AddTransient<ILocationService, LocationService>();
            services.AddTransient<IAdminEventRaiser, AdminEventRaiser>();
            services.AddEventGridClient(configuration);

            if (publicDataDbExists)
            {
                services.AddHttpClient<IProcessorClient, ProcessorClient>((provider, httpClient) =>
                {
                    var options = provider.GetRequiredService<IOptions<PublicDataProcessorOptions>>();
                    httpClient.BaseAddress = new Uri(options.Value.Url);
                    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, SecurityConstants.AdminUserAgent);
                });

                services.AddHttpClient<IPublicDataApiClient, PublicDataApiClient>((provider, httpClient) =>
                {
                    var options = provider.GetRequiredService<IOptions<PublicDataApiOptions>>();
                    httpClient.BaseAddress = new Uri(options.Value.PrivateUrl);
                    httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, SecurityConstants.AdminUserAgent);
                });

                services.AddTransient<IDataSetService, DataSetService>();
                services.AddTransient<IDataSetVersionService, DataSetVersionService>();
                services.AddTransient<IDataSetVersionMappingService, DataSetVersionMappingService>();
                services.AddTransient<IPreviewTokenService, PreviewTokenService>();
                services.AddTransient<IDataSetVersionRepository, DataSetVersionRepository>();
            }
            else
            {
                services.AddTransient<IProcessorClient, NoOpProcessorClient>();

                // TODO EES-5073 Remove this once PublicDataDbContext is configured in ALL Azure environments.
                // This is allowing for the PublicDataDbContext to be null.
                services.AddTransient<IDataSetService, DataSetService>(provider =>
                    new DataSetService(provider.GetRequiredService<ContentDbContext>(),
                        provider.GetService<PublicDataDbContext>()!,
                        provider.GetRequiredService<IProcessorClient>(),
                        provider.GetRequiredService<IUserService>()));

                services.AddTransient<IDataSetVersionService, NoOpDataSetVersionService>();
                services.AddTransient<IDataSetVersionMappingService, NoOpDataSetVersionMappingService>();
                services.AddTransient<IPreviewTokenService, NoOpPreviewTokenService>();
                services.AddTransient<IDataSetVersionRepository, NoOpDataSetVersionRepository>();
            }

            services.AddTransient<INotificationClient>(s =>
            {
                var notifyOptions = s.GetRequiredService<IOptions<NotifyOptions>>();

                var notifyApiKey = notifyOptions.Value.ApiKey;

                if (!hostEnvironment.IsDevelopment() && !hostEnvironment.IsIntegrationTest())
                {
                    return new NotificationClient(notifyApiKey);
                }

                if (!notifyApiKey.IsNullOrEmpty())
                {
                    return new NotificationClient(notifyApiKey);
                }

                var logger = s.GetRequiredService<ILogger<LoggingNotificationClient>>();
                return new LoggingNotificationClient(logger);
            });
            services.AddTransient<IEmailService, EmailService>();

            services.AddTransient<IBoundaryLevelService, BoundaryLevelService>();
            services.AddTransient<IBoundaryLevelRepository, BoundaryLevelRepository>();
            services.AddTransient<IEmailTemplateService, EmailTemplateService>();
            services.AddTransient<ITableBuilderService, TableBuilderService>();
            services.AddTransient<IFilterRepository, FilterRepository>();
            services.AddTransient<IFilterItemRepository, FilterItemRepository>();
            services.AddTransient<IFootnoteService, FootnoteService>();
            services.AddTransient<IFootnoteRepository, FootnoteRepository>();
            services.AddTransient<IBoundaryDataRepository, BoundaryDataRepository>();
            services.AddTransient<IGlossaryService, GlossaryService>();
            services.AddTransient<IIndicatorGroupRepository, IndicatorGroupRepository>();
            services.AddTransient<IIndicatorRepository, IndicatorRepository>();
            services.AddTransient<ILocationRepository, LocationRepository>();
            services.AddTransient<IDataGuidanceService, DataGuidanceService>();
            services.AddTransient<IDataGuidanceDataSetService, DataGuidanceDataSetService>();
            services.AddTransient<IObservationService, ObservationService>();
            services.AddTransient<Data.Services.Interfaces.IReleaseService, Data.Services.ReleaseService>();
            services.AddTransient<IContentSectionRepository, ContentSectionRepository>();
            services.AddTransient<IReleaseNoteService, ReleaseNoteService>();
            services.AddTransient<IReleaseRepository, ReleaseRepository>();
            services.AddTransient<Content.Model.Repository.Interfaces.IReleaseVersionRepository,
                Content.Model.Repository.ReleaseVersionRepository>();
            services.AddTransient<Content.Model.Repository.Interfaces.IPublicationRepository,
                Content.Model.Repository.PublicationRepository>();
            services.AddTransient<ISubjectRepository, SubjectRepository>();
            services.AddTransient<ITimePeriodService, TimePeriodService>();
            services.AddTransient<IReleaseSubjectService, ReleaseSubjectService>();
            services.AddTransient<ISubjectMetaService, SubjectMetaService>(provider =>
                new SubjectMetaService(
                    statisticsDbContext: provider.GetRequiredService<StatisticsDbContext>(),
                    contentDbContext: provider.GetRequiredService<ContentDbContext>(),
                    cacheService: provider.GetRequiredService<IPrivateBlobCacheService>(),
                    releaseSubjectService: provider.GetRequiredService<IReleaseSubjectService>(),
                    filterRepository: provider.GetRequiredService<IFilterRepository>(),
                    filterItemRepository: provider.GetRequiredService<IFilterItemRepository>(),
                    indicatorGroupRepository: provider.GetRequiredService<IIndicatorGroupRepository>(),
                    locationRepository: provider.GetRequiredService<ILocationRepository>(),
                    logger: provider.GetRequiredService<ILogger<SubjectMetaService>>(),
                    observationService: provider.GetRequiredService<IObservationService>(),
                    timePeriodService: provider.GetRequiredService<ITimePeriodService>(),
                    userService: provider.GetRequiredService<IUserService>(),
                    locationOptions: provider.GetRequiredService<IOptions<LocationsOptions>>()
                ));
            services.AddTransient<ISubjectResultMetaService, SubjectResultMetaService>();
            services.AddTransient<ISubjectCsvMetaService, SubjectCsvMetaService>();
            services.AddSingleton<DataServiceMemoryCache<BoundaryLevel>, DataServiceMemoryCache<BoundaryLevel>>();
            services.AddSingleton<DataServiceMemoryCache<BoundaryData>, DataServiceMemoryCache<BoundaryData>>();
            services.AddTransient<IUserManagementService, UserManagementService>();
            services.AddTransient<IReleaseInviteService, ReleaseInviteService>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserInviteRepository, UserInviteRepository>();
            services.AddTransient<IDataSetValidator, DataSetValidator>();
            services.AddTransient<IFileValidatorService, FileValidatorService>();
            services.AddTransient<IReleaseFileBlobService, PrivateReleaseFileBlobService>();
            services.AddTransient<IPrivateBlobStorageService, PrivateBlobStorageService>(provider =>
                new PrivateBlobStorageService(configuration.GetRequiredValue("CoreStorage"),
                    provider.GetRequiredService<ILogger<IBlobStorageService>>()));
            services.AddTransient<IPublicBlobStorageService, PublicBlobStorageService>(provider => 
                new PublicBlobStorageService(configuration.GetRequiredValue("PublicStorage"),
                    provider.GetRequiredService<ILogger<IBlobStorageService>>()));
            services.AddTransient<IPublisherTableStorageService, PublisherTableStorageService>(_ =>
                new PublisherTableStorageService(configuration.GetRequiredValue("PublisherStorage")));
            services.AddSingleton<IGuidGenerator, SequentialGuidGenerator>();
            AddPersistenceHelper<ContentDbContext>(services);
            AddPersistenceHelper<StatisticsDbContext>(services);
            AddPersistenceHelper<UsersAndRolesDbContext>(services);
            services.AddTransient<AuthorizationHandlerService>();
            services.AddScoped<DateTimeProvider>();

            // This service allows a set of users to be pre-invited to the service on startup.
            if (hostEnvironment.IsDevelopment())
            {
                services.AddTransient<BootstrapUsersService>();
            }

            // These services allow us to check our Policies within Controllers and Services
            StartupSecurityConfiguration.ConfigureResourceBasedAuthorization(services);

            services.AddSingleton<IFileTypeService, FileTypeService>();
            services.AddTransient<IPrivateBlobCacheService, PrivateBlobCacheService>();
            services.AddTransient<ICacheKeyService, CacheKeyService>();
            services.AddSingleton<IDataProcessorClient, DataProcessorClient>(_ =>
                new DataProcessorClient(configuration.GetRequiredValue("CoreStorage")));
            services.AddSingleton<IPublisherClient, PublisherClient>(_ =>
                new PublisherClient(configuration.GetRequiredValue("PublisherStorage")));

            /*
             * Swagger
             */
            if (configuration.GetValue<bool>("App:EnableSwagger"))
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1",
                        new OpenApiInfo
                        {
                            Title = "Explore education statistics - Admin API",
                            Version = "v1"
                        });
                    c.CustomSchemaIds((type) => type.FullName);
                    c.AddSecurityDefinition("Bearer",
                        new OpenApiSecurityScheme
                        {
                            Description =
                                "Please enter into field the word 'Bearer' followed by a space and the JWT contents",
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.ApiKey
                        });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            [string.Empty]
                        }
                    });
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var provider = app.ApplicationServices;

            // Enable caching and register any caching services.
            CacheAspect.Enabled = true;
            var privateCacheService = new BlobCacheService(
                app.ApplicationServices.GetRequiredService<IPrivateBlobStorageService>(),
                provider.GetRequiredService<ILogger<BlobCacheService>>()
            );
            var publicCacheService = new BlobCacheService(
                app.ApplicationServices.GetRequiredService<IPublicBlobStorageService>(),
                provider.GetRequiredService<ILogger<BlobCacheService>>()
            );
            BlobCacheAttribute.AddService("default", privateCacheService);
            BlobCacheAttribute.AddService("public", publicCacheService);

            if (!env.IsIntegrationTest())
            {
                UpdateDatabase(app, env);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts(opts =>
                {
                    opts.MaxAge(365);
                    opts.IncludeSubdomains();
                    opts.Preload();
                });
            }

            app.UseResponseCompression();

            if (configuration.GetValue<bool>("App:EnableSwagger"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin API V1");
                    c.RoutePrefix = "docs";
                });
            }

            // Security Headers
            app.UseXContentTypeOptions();
            app.UseXXssProtection(opts => opts.EnabledWithBlockMode());
            app.UseXfo(opts => opts.SameOrigin());
            app.UseReferrerPolicy(opts => opts.NoReferrerWhenDowngrade());
            app.UseCsp(opts => opts
                .BlockAllMixedContent()
                .StyleSources(s => s.Self())
                .StyleSources(s => s
                    .CustomSources(" https://cdnjs.cloudflare.com")
                    .UnsafeInline())
                .FontSources(s => s.Self())
                .FormActions(s =>
                {
                    var loginAuthorityUrl = configuration
                        .GetRequiredSection("OpenIdConnectIdentityFramework")
                        .GetRequiredValue("Authority");
                    var loginAuthorityUri = new Uri(loginAuthorityUrl);
                    s
                        .CustomSources(loginAuthorityUri.GetLeftPart(UriPartial.Authority))
                        .Self();
                })
                .FrameAncestors(s => s.Self())
                .ImageSources(s => s.Self())
                .ImageSources(s => s.CustomSources("data:"))
                .ScriptSources(s => s.Self())
                .ScriptSources(s => s.UnsafeInline())
            );

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();
            app.UseHealthChecks("/api/health");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<ReleaseContentHub>("/hubs/release-content");
                }
            );

            app.UseMvc();

            if (!env.IsIntegrationTest())
            {
                app.UseSpa(spa =>
                {
                    if (env.IsDevelopment())
                    {
                        spa.Options.SourcePath = "../explore-education-statistics-admin";
                        spa.UseReactDevelopmentServer("start");
                    }
                });
            }

            app.ServerFeatures.Get<IServerAddressesFeature>()
                ?.Addresses
                .ForEach(address => Console.WriteLine($"Server listening on address: {address}"));
        }

        private void UpdateDatabase(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetRequiredService<StatisticsDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }

                using (var context = serviceScope.ServiceProvider.GetRequiredService<UsersAndRolesDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }

                using (var context = serviceScope.ServiceProvider.GetRequiredService<ContentDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }

                if (!env.IsIntegrationTest())
                {
                    ApplyCustomMigrations(app);
                }
            }

            if (env.IsDevelopment())
            {
                using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

                serviceScope.ServiceProvider
                    .GetRequiredService<BootstrapUsersService>()
                    .AddBootstrapUsers();
            }
        }

        private static void ApplyCustomMigrations(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            var migrations = serviceScope.ServiceProvider.GetServices<ICustomMigration>();

            foreach (var migration in migrations)
            {
                migration.Apply();
            }
        }
    }

    internal class NoOpProcessorClient : IProcessorClient
    {
        public Task<Either<ActionResult, ProcessDataSetVersionResponseViewModel>> CreateDataSet(
            Guid releaseFileId,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Either<ActionResult, ProcessDataSetVersionResponseViewModel>> CreateNextDataSetVersionMappings(
            Guid dataSetId,
            Guid releaseFileId,
            Guid? dataSetVersionToReplaceId = null,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Either<ActionResult, ProcessDataSetVersionResponseViewModel>> CompleteNextDataSetVersionImport(
            Guid dataSetVersionId,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Either<ActionResult, Unit>> BulkDeleteDataSetVersions(
            Guid releaseVersionId,
            bool forceDeleteAll,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Either<ActionResult, Unit>(Unit.Instance));
        }

        public Task<Either<ActionResult, Unit>> DeleteDataSetVersion(
            Guid dataSetVersionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Either<ActionResult, Unit>(Unit.Instance));
        }
    }

    internal class NoOpDataSetVersionService : IDataSetVersionService
    {
        public Task<Either<ActionResult, PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>>> ListLiveVersions(
            Guid dataSetId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PaginatedListViewModel<DataSetLiveVersionSummaryViewModel>.Paginate([], 1, 10));
        }

        public Task<Either<ActionResult, DataSetVersionInfoViewModel>> GetDataSetVersion(
            Guid dataSetVersionIdId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Either<ActionResult, DataSetVersionInfoViewModel>(new NotFoundResult()));
        }

        public Task<Either<ActionResult, DataSetVersion>> GetDataSetVersion(
            Guid dataSetId,
            SemVersion version,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Either<ActionResult, DataSetVersion>(new NotFoundResult()));
        }

        public Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(
            Guid releaseVersionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<DataSetVersionStatusSummary>());
        }

        public Task<Either<ActionResult, DataSetVersionSummaryViewModel>> CreateNextVersion(
            Guid releaseFileId,
            Guid dataSetId,
            Guid? dataSetVersionToReplaceId = null,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Either<ActionResult, DataSetVersionSummaryViewModel>> CompleteNextVersionImport(
            Guid dataSetVersionId,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<Either<ActionResult, Unit>> DeleteVersion(
            Guid dataSetVersionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Either<ActionResult, Unit>(Unit.Instance));
        }

        public Task<Either<ActionResult, HttpResponseMessage>> GetVersionChanges(
            Guid dataSetVersionId,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Either<ActionResult, DataSetDraftVersionViewModel>> UpdateVersion(
            Guid dataSetVersionId,
            DataSetVersionUpdateRequest updateRequest,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new Either<ActionResult, DataSetDraftVersionViewModel>(new NotFoundResult()));
        }

        public Task UpdateVersionsForReleaseVersion(
            Guid releaseVersionId,
            string releaseSlug,
            string releaseTitle,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    internal class NoOpDataSetVersionMappingService : IDataSetVersionMappingService
    {
        public Task<Either<ActionResult, LocationMappingPlan>> GetLocationMappings(
            Guid nextDataSetVersionId,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Either<ActionResult, BatchLocationMappingUpdatesResponseViewModel>>
            ApplyBatchLocationMappingUpdates(
                Guid nextDataSetVersionId,
                BatchLocationMappingUpdatesRequest request,
                CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Either<ActionResult, FilterMappingPlan>> GetFilterMappings(
            Guid nextDataSetVersionId,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<Either<ActionResult, BatchFilterOptionMappingUpdatesResponseViewModel>>
            ApplyBatchFilterOptionMappingUpdates(Guid nextDataSetVersionId,
                BatchFilterOptionMappingUpdatesRequest request,
                CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    internal class NoOpPreviewTokenService : IPreviewTokenService
    {
        public Task<Either<ActionResult, PreviewTokenViewModel>> CreatePreviewToken(
            Guid dataSetVersionId,
            string label,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Either<ActionResult, PreviewTokenViewModel>> GetPreviewToken(
            Guid previewTokenId,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Either<ActionResult, IReadOnlyList<PreviewTokenViewModel>>> ListPreviewTokens(
            Guid dataSetVersionId,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<Either<ActionResult, PreviewTokenViewModel>> RevokePreviewToken(
            Guid previewTokenId,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }

    internal class NoOpDataSetVersionRepository : IDataSetVersionRepository
    {
        public Task<List<DataSetVersion>> GetDataSetVersions(Guid releaseVersionId)
        {
            return Task.FromResult(new List<DataSetVersion>());
        }
    }
}
