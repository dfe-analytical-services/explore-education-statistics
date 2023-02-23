#nullable enable
using System;
using System.Collections.Generic;
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
using GovUk.Education.ExploreEducationStatistics.Common.Database;
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
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Notify.Client;
using Notify.Interfaces;
using Thinktecture;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using IContentGlossaryService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IGlossaryService;
using ContentGlossaryService = GovUk.Education.ExploreEducationStatistics.Content.Services.GlossaryService;
using IContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IMethodologyService;
using ContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.MethodologyService;
using ContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.PublicationService;
using IContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;
using ContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.ReleaseService;
using DataGuidanceService = GovUk.Education.ExploreEducationStatistics.Admin.Services.DataGuidanceService;
using GlossaryService = GovUk.Education.ExploreEducationStatistics.Admin.Services.GlossaryService;
using IContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using IDataGuidanceService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IDataGuidanceService;
using IGlossaryService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IGlossaryService;
using IMethodologyImageService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies.IMethodologyImageService;
using IMethodologyService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies.IMethodologyService;
using IPublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationRepository;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IPublicationService;
using IReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseFileService;
using IReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseRepository;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using IThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IThemeService;
using MethodologyImageService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies.MethodologyImageService;
using MethodologyService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies.MethodologyService;
using PublicationRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationRepository;
using PublicationService = GovUk.Education.ExploreEducationStatistics.Admin.Services.PublicationService;
using ReleaseFileService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseFileService;
using ReleaseRepository = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseRepository;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseService;
using ThemeService = GovUk.Education.ExploreEducationStatistics.Admin.Services.ThemeService;

namespace GovUk.Education.ExploreEducationStatistics.Admin
{
    public class Startup
    {
        private const string OpenIdConnectSpaClientId = "GovUk.Education.ExploreEducationStatistics.Admin";

        private static readonly List<string> DevelopmentAdminUrlAliases = ListOf("https://ees.local:5021");

        private IConfiguration Configuration { get; }
        private IHostEnvironment HostEnvironment { get; }

        private readonly List<string> _adminUrlAndAliases;

        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;

            _adminUrlAndAliases = ListOf($"https://{Configuration.GetValue<string>("AdminUri")}");
            if (hostEnvironment.IsDevelopment())
            {
                _adminUrlAndAliases.AddRange(DevelopmentAdminUrlAliases);
            }
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
                options.CheckConsentNeeded = _ => true;
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
                        providerOptions =>
                            providerOptions
                                .MigrationsAssembly(typeof(Startup).Assembly.FullName)
                                .EnableCustomRetryOnFailure()
                            )
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            services.AddDbContext<ContentDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        providerOptions =>
                            providerOptions
                                .MigrationsAssembly(typeof(Startup).Assembly.FullName)
                                .EnableCustomRetryOnFailure()
                        )
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            services.AddDbContext<StatisticsDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("StatisticsDb"),
                        providerOptions =>
                            providerOptions
                                .MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model")
                                .AddBulkOperationSupport()
                                .EnableCustomRetryOnFailure()
                            )
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
                    var spaClient = options
                        .Clients
                        .First(client => client.ClientId == OpenIdConnectSpaClientId);

                    var clientConfig = Configuration.GetSection("OpenIdConnectSpaClient");

                    if (clientConfig == null)
                    {
                        return;
                    }

                    var allowRefreshTokens = clientConfig.GetValue("AllowOfflineAccess", false);

                    if (allowRefreshTokens)
                    {
                        // Allow the use of refresh tokens to add persistent access to the service and enable the silent
                        // login flow.
                        spaClient.AllowOfflineAccess = true;
                        spaClient.AllowedScopes = spaClient
                            .AllowedScopes
                            .Append(OpenIdConnectScope.OfflineAccess)
                            .ToList();

                        spaClient.UpdateAccessTokenClaimsOnRefresh = true;

                        var tokenUsage = clientConfig.GetValue<string>("RefreshTokenUsage");

                        spaClient.RefreshTokenUsage = tokenUsage != null
                            ? EnumUtil.GetFromString<TokenUsage>(tokenUsage)
                            : TokenUsage.OneTimeOnly;

                        var tokenExpiration = clientConfig.GetValue<string>("RefreshTokenExpiration");

                        spaClient.RefreshTokenExpiration = tokenExpiration != null
                            ? EnumUtil.GetFromString<TokenExpiration>(tokenExpiration)
                            : TokenExpiration.Absolute;
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
            }

            services.Configure<JwtBearerOptions>(
                IdentityServerJwtConstants.IdentityServerJwtBearerScheme,
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // When the user returns from logging into the Identity Provider (e.g. Azure AD, Keycloak etc)
                        // the external login portion of the Open ID Connect flow is completed, and then the Admin
                        // SPA and Identity Server (the locally running implementation of an Open ID Connect IdP) enter
                        // into a conversation, effectively swapping the access token issued from Azure or Keycloak
                        // for a new access token issued from Identity Server itself specifically for the SPA's use.
                        //
                        // The "Issuer" of this access token is by default whatever URL that the SPA used to initiate
                        // the conversation with Identity Server.  So if the external IdP returns the user to
                        // https://localhost:5021, then the SPA will use https://localhost:5021 as a basis for
                        // negotiating with Identity Server, and thus Identity Server will issue its access tokens with
                        // the "Issuer" set to "https://localhost:5021".
                        //
                        // However, by default Identity Server will set its "TokenValidationParameters.ValidIssuer"
                        // property to the "applicationUrl" value in "launchSettings.json" - locally for instance,
                        // this would be "https://0.0.0.0:5021".  This complicates matters further as access tokens
                        // that it issues would otherwise be immediately invalidated by this setting, regardless of what
                        // URL the user was hitting the site on.  The answer is to manually let Identity Server know
                        // which URLs are appropriate for each environment.
                        //
                        // As locally it's possible to access the service under an alternative URL like
                        // "https://ees.local:5021", then we need to ensure that both "https://localhost:5021" and
                        // "https://ees.local:5021" are *both* valid "Issuer" values on the access token provided by
                        // Identity Server.
                        ValidIssuers = _adminUrlAndAliases
                    };
                });

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
            services.Configure<ReleaseApprovalOptions>(
                Configuration.GetSection(ReleaseApprovalOptions.ReleaseApproval));
            services.Configure<TableBuilderOptions>(Configuration.GetSection(TableBuilderOptions.TableBuilder));
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            StartupSecurityConfiguration.ConfigureAuthorizationPolicies(services);

            /*
             * Services
             */

            var coreStorageConnectionString = Configuration.GetValue<string>("CoreStorage");
            var publisherStorageConnectionString = Configuration.GetValue<string>("PublisherStorage");

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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
            services.AddTransient<IReleasePublishingStatusService, ReleasePublishingStatusService>(s =>
                new ReleasePublishingStatusService(
                    s.GetService<IMapper>(),
                    s.GetService<IUserService>(),
                    s.GetService<IPersistenceHelper<ContentDbContext>>(),
                    new TableStorageService(
                        publisherStorageConnectionString,
                        new StorageInstanceCreationUtil())));
            services.AddTransient<IReleasePublishingStatusRepository, ReleasePublishingStatusRepository>(_ =>
                new ReleasePublishingStatusRepository(
                    new TableStorageService(
                        publisherStorageConnectionString,
                        new StorageInstanceCreationUtil())
                )
            );
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<ITopicService, TopicService>();
            services.AddTransient<IPublicationService, PublicationService>(provider =>
                new PublicationService(
                    context: provider.GetRequiredService<ContentDbContext>(),
                    mapper: provider.GetRequiredService<IMapper>(),
                    persistenceHelper: provider.GetRequiredService<IPersistenceHelper<ContentDbContext>>(),
                    userService: provider.GetRequiredService<IUserService>(),
                    publicationRepository: provider.GetRequiredService<IPublicationRepository>(),
                    methodologyVersionRepository: provider.GetRequiredService<IMethodologyVersionRepository>(),
                    methodologyCacheService: provider.GetRequiredService<IMethodologyCacheService>(),
                    publicationCacheService: provider.GetRequiredService<IPublicationCacheService>()
                )
            );
            services.AddTransient<IPublicationRepository, PublicationRepository>();
            services.AddTransient<IMetaService, MetaService>();
            services.AddTransient<ILegacyReleaseService, LegacyReleaseService>(provider =>
                new LegacyReleaseService(
                    context: provider.GetRequiredService<ContentDbContext>(),
                    mapper: provider.GetRequiredService<IMapper>(),
                    userService: provider.GetRequiredService<IUserService>(),
                    persistenceHelper: provider.GetRequiredService<IPersistenceHelper<ContentDbContext>>(),
                    publicationCacheService: provider.GetRequiredService<IPublicationCacheService>())
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
                    context: provider.GetRequiredService<ContentDbContext>(),
                    persistenceHelper: provider.GetRequiredService<IPersistenceHelper<ContentDbContext>>(),
                    methodologyContentService: provider.GetRequiredService<IMethodologyContentService>(),
                    methodologyFileRepository: provider.GetRequiredService<IMethodologyFileRepository>(),
                    methodologyImageService: provider.GetRequiredService<IMethodologyImageService>(),
                    methodologyVersionRepository: provider.GetRequiredService<IMethodologyVersionRepository>(),
                    publishingService: provider.GetRequiredService<IPublishingService>(),
                    userService: provider.GetRequiredService<IUserService>(),
                    methodologyCacheService: provider.GetRequiredService<IMethodologyCacheService>()));
            services.AddTransient<IDataBlockService, DataBlockService>();
            services.AddTransient<IPreReleaseUserService, PreReleaseUserService>();
            services.AddTransient<IPreReleaseService, PreReleaseService>();
            services.AddTransient<IPreReleaseSummaryService, PreReleaseSummaryService>();

            services.AddTransient<IManageContentPageService, ManageContentPageService>();
            services.AddTransient<IContentBlockService, ContentBlockService>();
            services.AddTransient<IContentService, ContentService>();
            services.AddTransient<IEmbedBlockService, EmbedBlockService>();
            services.AddTransient<IReleaseContentBlockService, ReleaseContentBlockService>();
            services.AddTransient<IKeyStatisticService, KeyStatisticService>();
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
            services.AddTransient<IReleaseContentBlockRepository, ReleaseContentBlockRepository>();
            services.AddTransient<IReleaseContentSectionRepository, ReleaseContentSectionRepository>();
            services.AddTransient<IReleaseNoteService, ReleaseNoteService>();
            services.AddTransient<Content.Model.Repository.Interfaces.IReleaseRepository,
                Content.Model.Repository.ReleaseRepository>();
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
            services.AddTransient(provider => GetBlobStorageService(provider, "CoreStorage"));
            services.AddTransient<ITableStorageService, TableStorageService>(_ =>
                new TableStorageService(
                    coreStorageConnectionString,
                    new StorageInstanceCreationUtil()));
            services.AddTransient<IStorageQueueService, StorageQueueService>(_ =>
                new StorageQueueService(
                    coreStorageConnectionString,
                    new StorageInstanceCreationUtil()));
            services.AddTransient<IDataBlockMigrationService, DataBlockMigrationService>();
            services.AddSingleton<IGuidGenerator, SequentialGuidGenerator>();
            AddPersistenceHelper<ContentDbContext>(services);
            AddPersistenceHelper<StatisticsDbContext>(services);
            AddPersistenceHelper<UsersAndRolesDbContext>(services);
            services.AddTransient<AuthorizationHandlerResourceRoleService>();
            services.AddScoped<DateTimeProvider>();

            // TODO EES-3755 Remove after Permalink snapshot migration work is complete
            services.AddTransient<IPermalinkMigrationService, PermalinkMigrationService>(provider =>
                new PermalinkMigrationService(
                    storageQueueService: new StorageQueueService(
                        Configuration.GetValue<string>("PublisherStorage"),
                        new StorageInstanceCreationUtil()),
                    userService: provider.GetRequiredService<IUserService>()));

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
                    publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                    glossaryCacheService: provider.GetRequiredService<IGlossaryCacheService>(),
                    methodologyCacheService: provider.GetRequiredService<IMethodologyCacheService>(),
                    publicationCacheService: provider.GetRequiredService<IPublicationCacheService>()
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
            var provider = app.ApplicationServices;

            // Enable caching and register any caching services.
            CacheAspect.Enabled = true;
            var privateBlobCacheService = GetBlobCacheService(provider, "CoreStorage");
            var publicBlobCacheService = GetBlobCacheService(provider, "PublicStorage");
            BlobCacheAttribute.AddService("default", privateBlobCacheService);
            BlobCacheAttribute.AddService("public", publicBlobCacheService);

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

            // Deny access to all /Identity routes other than:
            //
            // /Identity/Account/Login
            // /Identity/Account/ExternalLogin
            // /Identity/Account/InviteExpired
            //
            // This Regex is case insensitive.
            var options = new RewriteOptions()
                .AddRewrite(
                    @"^(?i)identity/(?!account/(login|externallogin|inviteexpired))",
                    replacement: "/",
                    skipRemainingRules: true);
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
                    .GetRequiredService<BootstrapUsersService>()
                    .AddBootstrapUsers();
            }
        }

        private IBlobCacheService GetBlobCacheService(IServiceProvider provider, string connectionStringKey)
        {
            return new BlobCacheService(
                blobStorageService: GetBlobStorageService(provider, connectionStringKey),
                logger: provider.GetRequiredService<ILogger<BlobCacheService>>());
        }

        private IBlobStorageService GetBlobStorageService(IServiceProvider provider, string connectionStringKey)
        {
            var connectionString = Configuration.GetValue<string>(connectionStringKey);
            return new BlobStorageService(
                connectionString,
                new BlobServiceClient(connectionString),
                provider.GetRequiredService<ILogger<BlobStorageService>>(),
                new StorageInstanceCreationUtil());
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
