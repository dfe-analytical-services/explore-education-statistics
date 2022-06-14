#nullable enable
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Pages.Account;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Bau;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Filters;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings.Interfaces;
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
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Interfaces;
using Thinktecture;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using FootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.FootnoteService;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IFootnoteService;
using IDataGuidanceService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IDataGuidanceService;
using IMethodologyImageService =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies.IMethodologyImageService;
using IMethodologyService =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies.IMethodologyService;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationService;
using IReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseFileService;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using IThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IThemeService;
using DataGuidanceService = GovUk.Education.ExploreEducationStatistics.Admin.Services.DataGuidanceService;
using GlossaryService = GovUk.Education.ExploreEducationStatistics.Admin.Services.GlossaryService;
using IGlossaryService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IGlossaryService;
using MethodologyImageService =
    GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies.MethodologyImageService;
using MethodologyService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies.MethodologyService;
using PublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;
using ReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseFileService;
using ReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseRepository;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseService;
using ThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ThemeService;

namespace GovUk.Education.ExploreEducationStatistics.Admin
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IHostEnvironment HostEnvironment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
            });

            services
                .AddControllers(
                    options => { options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(",")); }
                )
                .AddControllersAsServices();

            services.AddHttpContextAccessor();

            services.AddMvc(options =>
                {
                    options.Filters.Add(new AuthorizeFilter(SecurityPolicies.CanAccessSystem.ToString()));
                    options.Filters.Add(new OperationCancelledExceptionFilter());
                    options.EnableEndpointRouting = false;
                    options.AllowEmptyInputInBodyModelBinding = true;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            // Adds Brotli and Gzip compressing
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "wwwroot"; });
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            /*
             * Database contexts
             */

            services.AddDbContext<UsersAndRolesDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            services.AddDbContext<ContentDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        builder => { builder.MigrationsAssembly(typeof(Startup).Assembly.FullName); })
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            services.AddDbContext<StatisticsDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("StatisticsDb"),
                        builder =>
                        {
                            builder.MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model");
                            builder.AddBulkOperationSupport();
                        })
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            /*
             * Auth / IdentityServer
             */

            // remove default Microsoft remapping of the name of the OpenID "roles" claim mapping
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("roles");
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

            services
                .AddDefaultIdentity<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<UsersAndRolesDbContext>()
                .AddDefaultTokenProviders();

            var identityServerConfig = services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddApiAuthorization<ApplicationUser, UsersAndRolesDbContext>(options =>
                {
                    var defaultClient = options
                        .Clients
                        .First(client => client.ClientId == "GovUk.Education.ExploreEducationStatistics.Admin");

                    // Allow the use of refresh tokens to add persistent access to the service and enable the silent
                    // login flow.
                    defaultClient.AllowOfflineAccess = true;
                    defaultClient.AllowedScopes = defaultClient.AllowedScopes
                        .Append(IdentityServerConstants.StandardScopes.OfflineAccess).ToList();

                    // TODO DW - clean this up
                    if (HostEnvironment.IsDevelopment())
                    {
                        defaultClient.RefreshTokenUsage = TokenUsage.ReUse;      
                    }
                })
                .AddProfileService<ApplicationUserProfileService>();
                
            if (HostEnvironment.IsDevelopment())
            {
                identityServerConfig.AddDeveloperSigningCredential();
            }
            else
            {
                identityServerConfig.AddSigningCredentials();

                services.Configure<JwtBearerOptions>(
                    IdentityServerJwtConstants.IdentityServerJwtBearerScheme,
                    options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = false,
                            ValidateIssuer = false,
                        };
                    });
            }

            services
                .AddAuthentication()
                .AddOpenIdConnect(options => Configuration.GetSection("OpenIdConnect").Bind(options))
                .AddIdentityServerJwt();

            services.Configure<JwtBearerOptions>(
                IdentityServerJwtConstants.IdentityServerJwtBearerScheme,
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
                        
                        // Allows requests with `access_token` query parameter to authenticate.
                        // Only really needed for websockets as we unfortunately can't set any
                        // headers in the browser for the initial handshake.
                        if (context.Request.Query.ContainsKey("access_token"))
                        {
                            context.Token = context.Request.Query["access_token"];
                        }
                    };
                });

            // This configuration has to occur after the AddAuthentication() block as it is otherwise overridden.
            services.Configure<IdentityOptions>(options =>
            {
                // This config tells UserManager to expect the logged in user's id to be in a Claim called
                // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" rather than "sub" (because
                // this Claim is renamed via the DefaultInboundClaimTypeMap earlier in the login process).
                //
                // It doesn't seem to be possible to remove the renaming (via
                // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub")) because some mechanism earlier in the
                // authentication process requires it to be in the Claim named
                // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" rather than "sub".
                options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;

                // Default User settings
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+'";
            });

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

            var azureSignalRConnectionString = Configuration.GetValue<string>("Azure:SignalR:ConnectionString");

            if (!azureSignalRConnectionString.IsNullOrEmpty())
            {
                signalRBuilder.AddAzureSignalR(azureSignalRConnectionString);
            }

            /*
             * Configuration options
             */

            services.Configure<PreReleaseOptions>(Configuration);
            services.Configure<LocationsOptions>(Configuration.GetSection(LocationsOptions.Locations));
            services.Configure<TableBuilderOptions>(Configuration.GetSection(TableBuilderOptions.TableBuilder));
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            StartupSecurityConfiguration.ConfigureAuthorizationPolicies(services);

            /*
             * Services
             */

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            services.AddTransient<IMyReleasePermissionsResolver,
                MyReleasePermissionsResolver>();
            services.AddTransient<IMyPublicationPermissionsResolver,
                MyPublicationPermissionsResolver>();
            services.AddTransient<IMyPublicationMethodologyVersionPermissionsResolver,
                MyPublicationMethodologyVersionPermissionsResolver>();
            services.AddTransient<IMyMethodologyVersionPermissionsResolver,
                MyMethodologyVersionPermissionsResolver>();

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
                    new StorageQueueService(Configuration.GetValue<string>("PublisherStorage")),
                    provider.GetService<IUserService>(),
                    provider.GetRequiredService<ILogger<PublishingService>>()));
            services.AddTransient<IReleasePublishingStatusService, ReleasePublishingStatusService>(s =>
                new ReleasePublishingStatusService(
                    s.GetService<IMapper>(),
                    s.GetService<IUserService>(),
                    s.GetService<IPersistenceHelper<ContentDbContext>>(),
                    new TableStorageService(Configuration.GetValue<string>("PublisherStorage"))));
            services.AddTransient<IReleasePublishingStatusRepository, ReleasePublishingStatusRepository>(s =>
                new ReleasePublishingStatusRepository(
                    new TableStorageService(Configuration.GetValue<string>("PublisherStorage"))
                )
            );
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<ITopicService, TopicService>();
            services.AddTransient<IPublicationService, PublicationService>(provider =>
                new PublicationService(
                    context: provider.GetService<ContentDbContext>(),
                    mapper: provider.GetService<IMapper>(),
                    persistenceHelper: provider.GetService<IPersistenceHelper<ContentDbContext>>(),
                    userService: provider.GetService<IUserService>(),
                    publicationRepository: provider.GetService<IPublicationRepository>(),
                    methodologyVersionRepository: provider.GetService<IMethodologyVersionRepository>(),
                    publicBlobCacheService: new BlobCacheService(
                        GetBlobStorageService(provider, "PublicStorage"),
                        provider.GetRequiredService<ILogger<BlobCacheService>>())
                )
            );
            services.AddTransient<IPublicationRepository, PublicationRepository>();
            services.AddTransient<IMetaService, MetaService>();
            services.AddTransient<ILegacyReleaseService, LegacyReleaseService>(provider =>
                new LegacyReleaseService(
                    context: provider.GetService<ContentDbContext>(),
                    mapper: provider.GetService<IMapper>(),
                    userService: provider.GetService<IUserService>(),
                    persistenceHelper: provider.GetService<IPersistenceHelper<ContentDbContext>>(),
                    publicBlobCacheService: new BlobCacheService(
                        GetBlobStorageService(provider, "PublicStorage"),
                        provider.GetRequiredService<ILogger<BlobCacheService>>())
                )
            );
            services.AddTransient<IReleaseService, ReleaseService>();
            services.AddTransient<IReleaseApprovalService, ReleaseApprovalService>();
            services.AddTransient<ReleaseSubjectRepository.SubjectDeleter, ReleaseSubjectRepository.SubjectDeleter>();
            services.AddTransient<IReleaseSubjectRepository, ReleaseSubjectRepository>();
            services.AddTransient<IReleaseChecklistService, ReleaseChecklistService>();
            services.AddTransient<IReleaseRepository, ReleaseRepository>();
            services.AddTransient<IMethodologyService, MethodologyService>();
            services.AddTransient<IMethodologyNoteService, MethodologyNoteService>();
            services.AddTransient<IMethodologyNoteRepository, MethodologyNoteRepository>();
            services.AddTransient<IMethodologyVersionRepository, MethodologyVersionRepository>();
            services.AddTransient<IMethodologyRepository, MethodologyRepository>();
            services.AddTransient<IMethodologyContentService, MethodologyContentService>();
            services.AddTransient<IMethodologyFileRepository, MethodologyFileRepository>();
            services.AddTransient<IMethodologyImageService, MethodologyImageService>();
            services.AddTransient<IMethodologyAmendmentService, MethodologyAmendmentService>();
            services.AddTransient<IMethodologyApprovalService, MethodologyApprovalService>(provider =>
                new MethodologyApprovalService(
                    context: provider.GetService<ContentDbContext>(),
                    persistenceHelper: provider.GetService<IPersistenceHelper<ContentDbContext>>(),
                    methodologyContentService: provider.GetRequiredService<IMethodologyContentService>(),
                    methodologyFileRepository: provider.GetRequiredService<IMethodologyFileRepository>(),
                    methodologyImageService: provider.GetRequiredService<IMethodologyImageService>(),
                    methodologyVersionRepository: provider.GetRequiredService<IMethodologyVersionRepository>(),
                    publicBlobCacheService: new BlobCacheService(
                        GetBlobStorageService(provider, "PublicStorage"),
                        provider.GetRequiredService<ILogger<BlobCacheService>>()),
                    publishingService: provider.GetRequiredService<IPublishingService>(),
                    userService: provider.GetRequiredService<IUserService>()));
            services.AddTransient<IDataBlockService, DataBlockService>();
            services.AddTransient<IPreReleaseUserService, PreReleaseUserService>();
            services.AddTransient<IPreReleaseService, PreReleaseService>();
            services.AddTransient<IPreReleaseSummaryService, PreReleaseSummaryService>();

            services.AddTransient<IManageContentPageService, ManageContentPageService>();
            services.AddTransient<IContentService, ContentService>();
            services.AddTransient<IReleaseContentBlockService, ReleaseContentBlockService>();
            services.AddTransient<ICommentService, CommentService>();
            services.AddTransient<IRelatedInformationService, RelatedInformationService>();
            services.AddTransient<IReplacementService, ReplacementService>();
            services.AddTransient<IUserRoleService, UserRoleService>();
            services.AddTransient<IUserReleaseRoleService, UserReleaseRoleService>();
            services.AddTransient<IUserPublicationRoleRepository, UserPublicationRoleRepository>();
            services.AddTransient<IUserReleaseRoleRepository, UserReleaseRoleRepository>();
            services.AddTransient<IUserReleaseInviteRepository, UserReleaseInviteRepository>();
            services.AddTransient<IUserPublicationInviteRepository, UserPublicationInviteRepository>();

            services.AddTransient<INotificationClient>(s =>
            {
                var notifyApiKey = Configuration.GetValue<string>("NotifyApiKey");

                if (!HostEnvironment.IsDevelopment())
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
            services.AddTransient<IDataGuidanceSubjectService, DataGuidanceSubjectService>();
            services.AddTransient<IObservationService, ObservationService>();
            services.AddTransient<Data.Services.Interfaces.IReleaseService, Data.Services.ReleaseService>();
            // TODO: EES-2343 Remove when file sizes are stored in database
            services.AddTransient<Data.Services.Interfaces.IReleaseService.IBlobInfoGetter, ReleaseBlobInfoGetter>();
            services.AddTransient<IReleaseContentBlockRepository, ReleaseContentBlockRepository>();
            services.AddTransient<IReleaseContentSectionRepository, ReleaseContentSectionRepository>();
            services.AddTransient<IReleaseNoteService, ReleaseNoteService>();
            services.AddTransient<IResultBuilder<Observation, ObservationViewModel>, ResultBuilder>();
            services
                .AddTransient<Data.Model.Repository.Interfaces.IReleaseRepository,
                    Data.Model.Repository.ReleaseRepository>();
            services.AddTransient<ISubjectRepository, SubjectRepository>();
            services.AddTransient<ITimePeriodService, TimePeriodService>();
            services.AddTransient<ISubjectMetaService, SubjectMetaService>();
            services.AddTransient<IResultSubjectMetaService, ResultSubjectMetaService>();
            services.AddSingleton<DataServiceMemoryCache<BoundaryLevel>, DataServiceMemoryCache<BoundaryLevel>>();
            services.AddSingleton<DataServiceMemoryCache<GeoJson>, DataServiceMemoryCache<GeoJson>>();
            services.AddTransient<IUserManagementService, UserManagementService>();
            services.AddTransient<IReleaseInviteService, ReleaseInviteService>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserInviteRepository, UserInviteRepository>();
            services.AddTransient<IFileUploadsValidatorService, FileUploadsValidatorService>();
            services.AddTransient(provider => GetBlobStorageService(provider, "CoreStorage"));
            services.AddTransient<ITableStorageService, TableStorageService>(s =>
                new TableStorageService(Configuration.GetValue<string>("CoreStorage")));
            services.AddTransient<IStorageQueueService, StorageQueueService>(s =>
                new StorageQueueService(Configuration.GetValue<string>("CoreStorage")));
            services.AddTransient<IDataBlockMigrationService, DataBlockMigrationService>();
            services.AddSingleton<IGuidGenerator, SequentialGuidGenerator>();
            AddPersistenceHelper<ContentDbContext>(services);
            AddPersistenceHelper<StatisticsDbContext>(services);
            AddPersistenceHelper<UsersAndRolesDbContext>(services);
            services.AddTransient<AuthorizationHandlerResourceRoleService>();

            // This service handles the generation of the JWTs for users after they log in
            services.AddTransient<IProfileService, ApplicationUserProfileService>();

            // These services act as delegates through to underlying Identity services that cannot be mocked or are
            // hard to mock.
            services.AddTransient<ISignInManagerDelegate, SignInManagerDelegate>();
            services.AddTransient<IUserManagerDelegate, UserManagerDelegate>();

            // This service allows a set of users to be pre-invited to the service on startup.
            if (HostEnvironment.IsDevelopment())
            {
                services.AddTransient<BootstrapUsersService>();
            }

            // These services allow us to check our Policies within Controllers and Services
            StartupSecurityConfiguration.ConfigureResourceBasedAuthorization(services);

            services.AddSingleton<IFileTypeService, FileTypeService>();
            services.AddTransient<IDataArchiveValidationService, DataArchiveValidationService>();
            services.AddTransient<IBlobCacheService, BlobCacheService>();
            services.AddTransient<ICacheKeyService, CacheKeyService>();

            // Register any controllers that need specific dependencies
            services.AddTransient(
                provider => new BauCacheController(
                    privateBlobStorageService: GetBlobStorageService(provider, "CoreStorage"),
                    publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage")
                )
            );

            /*
             * Swagger
             */

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo { Title = "Explore education statistics - Admin API", Version = "v1" });
                c.CustomSchemaIds((type) => type.FullName);
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Please enter into field the word 'Bearer' followed by a space and the JWT contents",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        new[] { string.Empty }
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable caching and register any caching services.
            CacheAspect.Enabled = true;
            BlobCacheAttribute.AddService("default", app.ApplicationServices.GetService<IBlobCacheService>());

            UpdateDatabase(app, env);

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

            if (Configuration.GetValue<bool>("enableSwagger"))
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
                    var loginAuthorityUrl = Configuration.GetSection("OpenIdConnect").GetValue<string>("Authority");
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
            app.UseIdentityServer();
            app.UseAuthorization();

            // deny access to all Identity routes other than /Identity/Account/Login and
            // /Identity/Account/ExternalLogin
            var options = new RewriteOptions()
                .AddRewrite(@"^(?i)identity/(?!account/(?:external)*login$)", "/", skipRemainingRules: true);
            app.UseRewriter(options);

            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<ReleaseContentHub>("/hubs/release-content");
                }
            );

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Areas",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                if (env.IsDevelopment())
                {
                    spa.Options.SourcePath = "../explore-education-statistics-admin";
                    spa.UseReactDevelopmentServer("start");
                }
            });
        }

        private void UpdateDatabase(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<StatisticsDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }

                using (var context = serviceScope.ServiceProvider.GetService<UsersAndRolesDbContext>())
                {
                    context.Database.SetCommandTimeout(int.MaxValue);
                    context.Database.Migrate();
                }

                using (var context = serviceScope.ServiceProvider.GetService<ContentDbContext>())
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
                    .GetService<BootstrapUsersService>()
                    .AddBootstrapUsers();
            }
        }

        private IBlobStorageService GetBlobStorageService(IServiceProvider provider, string connectionStringKey)
        {
            var connectionString = Configuration.GetValue<string>(connectionStringKey);
            return new BlobStorageService(
                connectionString,
                new BlobServiceClient(connectionString),
                provider.GetRequiredService<ILogger<BlobStorageService>>());
        }

        private static void ApplyCustomMigrations(params ICustomMigration[] migrations)
        {
            foreach (var migration in migrations)
            {
                migration.Apply();
            }
        }
    }
}
