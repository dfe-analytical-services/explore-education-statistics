#nullable enable
using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Filters;
using GovUk.Education.ExploreEducationStatistics.Admin.Migrations.Custom;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
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
using GovUk.Education.ExploreEducationStatistics.Admin.Settings;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
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
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Interfaces;
using Npgsql;
using Thinktecture;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using ContentGlossaryService = GovUk.Education.ExploreEducationStatistics.Content.Services.GlossaryService;
using ContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.MethodologyService;
using ContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.PublicationService;
using ContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.ReleaseService;
using DataGuidanceService = GovUk.Education.ExploreEducationStatistics.Admin.Services.DataGuidanceService;
using DataSetService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data.DataSetService;
using GlossaryService = GovUk.Education.ExploreEducationStatistics.Admin.Services.GlossaryService;
using IContentGlossaryService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IGlossaryService;
using IContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IMethodologyService;
using IContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using IContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;
using IDataGuidanceService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IDataGuidanceService;
using IDataSetService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data.IDataSetService;
using IGlossaryService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IGlossaryService;
using IMethodologyImageService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies.IMethodologyImageService;
using IMethodologyService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies.IMethodologyService;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationService;
using IReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseFileService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using IReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionRepository;
using IThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IThemeService;
using MethodologyImageService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies.MethodologyImageService;
using MethodologyService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies.MethodologyService;
using PublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationRepository;
using PublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;
using ReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseFileService;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseService;
using ReleaseVersionRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionRepository;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;
using ThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ThemeService;

namespace GovUk.Education.ExploreEducationStatistics.Admin
{
    public class Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
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
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });
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
                var publicDataDbConnectionString = configuration.GetConnectionString("PublicDataDb");

                if (hostEnvironment.IsDevelopment())
                {
                    // TODO EES-5073 Remove this check when the Public Data db is available in all Azure environments.
                    if (publicDataDbExists)
                    {
                        var dataSourceBuilder = new NpgsqlDataSourceBuilder(publicDataDbConnectionString);

                        // Set up the data source outside the `AddDbContext` action as this
                        // prevents `ManyServiceProvidersCreatedWarning` warnings due to EF
                        // creating over 20 `IServiceProvider` instances.
                        var dbDataSource = dataSourceBuilder.Build();

                        services.AddDbContext<PublicDataDbContext>(options =>
                        {
                            options
                                .UseNpgsql(dbDataSource)
                                .EnableSensitiveDataLogging(hostEnvironment.IsDevelopment());
                        });
                    }
                }
                else
                {
                    // TODO EES-5073 Remove this check when the Public Data db is available in all Azure environments.
                    if (publicDataDbExists)
                    {
                        services.AddDbContext<PublicDataDbContext>(options =>
                        {
                            var sqlServerTokenProvider = new DefaultAzureCredential();
                            var accessToken = sqlServerTokenProvider.GetToken(
                                    new TokenRequestContext(scopes:
                                        ["https://ossrdbms-aad.database.windows.net/.default"]))
                                .Token;
                            var connectionStringWithAccessToken =
                                publicDataDbConnectionString.Replace("[access_token]", accessToken);
                            var dbDataSource = new NpgsqlDataSourceBuilder(connectionStringWithAccessToken).Build();
                            options.UseNpgsql(dbDataSource);
                        });
                    }
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
                .AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddUserStore<UserStore<ApplicationUser, IdentityRole, UsersAndRolesDbContext>>()
                .AddRoleStore<RoleStore<IdentityRole, UsersAndRolesDbContext>>()
                .AddEntityFrameworkStores<UsersAndRolesDbContext>();

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

            services.Configure<PublicDataProcessorOptions>(configuration.GetRequiredSection(PublicDataProcessorOptions.Section));
            services.Configure<PreReleaseOptions>(configuration);
            services.Configure<LocationsOptions>(configuration.GetRequiredSection(LocationsOptions.Locations));
            services.Configure<ReleaseApprovalOptions>(
                configuration.GetRequiredSection(ReleaseApprovalOptions.ReleaseApproval));
            services.Configure<TableBuilderOptions>(configuration.GetRequiredSection(TableBuilderOptions.TableBuilder));
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.Configure<OpenIdConnectSpaClientOptions>(configuration.GetSection(
                OpenIdConnectSpaClientOptions.OpenIdConnectSpaClient));

            StartupSecurityConfiguration.ConfigureAuthorizationPolicies(services, configuration);

            /*
             * Services
             */

            var coreStorageConnectionString = configuration.GetValue<string>("CoreStorage");
            var publisherStorageConnectionString = configuration.GetValue<string>("PublisherStorage");

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
            services.AddTransient<IDataGuidanceFileWriter, DataGuidanceFileWriter>();
            services.AddTransient<IReleaseFileService, ReleaseFileService>();
            services.AddTransient<IReleaseImageService, ReleaseImageService>();
            services.AddTransient<IReleasePermissionService, ReleasePermissionService>();
            services.AddTransient<IDataImportService, DataImportService>();
            services.AddTransient<IImportStatusBauService, ImportStatusBauService>();

            services.AddTransient<IPublishingService, PublishingService>(provider =>
                new PublishingService(
                    provider.GetService<IPersistenceHelper<ContentDbContext>>(),
                    new StorageQueueService(
                        publisherStorageConnectionString,
                        new StorageInstanceCreationUtil()),
                    provider.GetService<IUserService>(),
                    provider.GetRequiredService<ILogger<PublishingService>>()));
            services.AddTransient<IReleasePublishingStatusService, ReleasePublishingStatusService>();
            services.AddTransient<IReleasePublishingStatusRepository, ReleasePublishingStatusRepository>();
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<ITopicService, TopicService>();
            services.AddTransient<IPublicationService, PublicationService>();
            services.AddTransient<IPublicationRepository, PublicationRepository>();
            services.AddTransient<IMetaService, MetaService>();
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
            services.AddTransient<Services.Interfaces.Public.Data.IReleaseService, Services.Public.Data.ReleaseService>();

            services.AddHttpClient<IProcessorClient, ProcessorClient>((provider, httpClient) =>
            {
                var options = provider.GetRequiredService<IOptions<PublicDataProcessorOptions>>();
                httpClient.BaseAddress = new Uri(options.Value.Url);
                httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "EES Admin");
            });

            if (publicDataDbExists)
            {
                services.AddTransient<IDataSetService, DataSetService>();
                services.AddTransient<IDataSetVersionService, DataSetVersionService>();
            }
            else
            {
                // TODO EES-5073 Remove this once PublicDataDbContext is configured in ALL Azure environments.
                // This is allowing for the PublicDataDbContext to be null.
                services.AddTransient<IDataSetService, DataSetService>(provider =>
                    new DataSetService(provider.GetRequiredService<ContentDbContext>(),
                        provider.GetService<PublicDataDbContext>(),
                        provider.GetRequiredService<IProcessorClient>(),
                        provider.GetRequiredService<IUserService>()));
                
                services.AddTransient<IDataSetVersionService, NoOpDataSetVersionService>();
            }

            services.AddTransient<INotificationClient>(s =>
            {
                var notifyApiKey = configuration.GetValue<string>("NotifyApiKey");

                if (!hostEnvironment.IsDevelopment() && !hostEnvironment.IsIntegrationTest())
                {
                    return new NotificationClient(notifyApiKey);
                }

                if (notifyApiKey != null && notifyApiKey != "change-me")
                {
                    return new NotificationClient(notifyApiKey);
                }

                var logger = s.GetRequiredService<ILogger<LoggingNotificationClient>>();
                return new LoggingNotificationClient(logger);
            });
            services.AddTransient<IEmailService, EmailService>();

            services.AddTransient<IBoundaryLevelRepository, BoundaryLevelRepository>();
            services.AddTransient<IEmailTemplateService, EmailTemplateService>();
            services.AddTransient<ITableBuilderService, TableBuilderService>();
            services.AddTransient<IFilterRepository, FilterRepository>();
            services.AddTransient<IFilterItemRepository, FilterItemRepository>();
            services.AddTransient<IFootnoteService, FootnoteService>();
            services.AddTransient<IFootnoteRepository, FootnoteRepository>();
            services.AddTransient<IGeoJsonRepository, GeoJsonRepository>();
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
            services.AddTransient<Content.Model.Repository.Interfaces.IReleaseVersionRepository,
                Content.Model.Repository.ReleaseVersionRepository>();
            services.AddTransient<Content.Model.Repository.Interfaces.IPublicationRepository,
                Content.Model.Repository.PublicationRepository>();
            services.AddTransient<ISubjectRepository, SubjectRepository>();
            services.AddTransient<ITimePeriodService, TimePeriodService>();
            services.AddTransient<IReleaseSubjectService, ReleaseSubjectService>();
            services.AddTransient<ISubjectMetaService, SubjectMetaService>();
            services.AddTransient<ISubjectResultMetaService, SubjectResultMetaService>();
            services.AddTransient<ISubjectCsvMetaService, SubjectCsvMetaService>();
            services.AddSingleton<DataServiceMemoryCache<BoundaryLevel>, DataServiceMemoryCache<BoundaryLevel>>();
            services.AddSingleton<DataServiceMemoryCache<GeoJson>, DataServiceMemoryCache<GeoJson>>();
            services.AddTransient<IUserManagementService, UserManagementService>();
            services.AddTransient<IReleaseInviteService, ReleaseInviteService>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserInviteRepository, UserInviteRepository>();
            services.AddTransient<IFileUploadsValidatorService, FileUploadsValidatorService>();
            services.AddTransient<IReleaseFileBlobService, PrivateReleaseFileBlobService>();

            services.AddSingleton<IPrivateBlobStorageService, PrivateBlobStorageService>();
            services.AddSingleton<IPublicBlobStorageService, PublicBlobStorageService>();

            services.AddTransient<ICoreTableStorageService, CoreTableStorageService>();
            services.AddTransient<IPublisherTableStorageService, PublisherTableStorageService>();
            services.AddTransient<IStorageQueueService, StorageQueueService>(_ =>
                new StorageQueueService(
                    coreStorageConnectionString,
                    new StorageInstanceCreationUtil()));
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
            services.AddTransient<IDataArchiveValidationService, DataArchiveValidationService>();
            services.AddTransient<IBlobCacheService, BlobCacheService>(provider =>
                new BlobCacheService(
                    provider.GetRequiredService<IPrivateBlobStorageService>(),
                    provider.GetRequiredService<ILogger<BlobCacheService>>()
                ));
            services.AddTransient<ICacheKeyService, CacheKeyService>();

            /*
             * Swagger
             */

            if (configuration.GetValue<bool>("enableSwagger"))
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1",
                        new OpenApiInfo {Title = "Explore education statistics - Admin API", Version = "v1"});
                    c.CustomSchemaIds((type) => type.FullName);
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
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
                                Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}
                            },
                            new[] {string.Empty}
                        }
                    });
                });
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

            if (configuration.GetValue<bool>("enableSwagger"))
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
                    var loginAuthorityUrl = configuration.GetRequiredSection("OpenIdConnectIdentityFramework").GetValue<string>("Authority");
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

                    ApplyCustomMigrations();
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

        private static void ApplyCustomMigrations(params ICustomMigration[] migrations)
        {
            foreach (var migration in migrations)
            {
                migration.Apply();
            }
        }
    }

    internal class NoOpDataSetVersionService : IDataSetVersionService
    {
        public Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(Guid releaseVersionId)
        {
            return Task.FromResult(new List<DataSetVersionStatusSummary>());
        } 
    }
}
