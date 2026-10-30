# Azure Resource update plan for ELZ

The plan for moving Explore Education Statistics (EES) to the new **s292** subscription under the DfE Enterprise Landing Zone (ELZ), tracked under EES-7502. It covers how resources will be named, how the network will be laid out, and which resource types and SKUs change on the way.

Most of this can be done while we wait on the platform team, because it doesn't depend on IP ranges.

**Status labels used below**
- **Decided**: agreed; build it this way.
- **Proposed**: recommended, awaiting a decision.
- **Blocked**: waiting on someone outside the team.

## Contents
1. [Key decisions](#1-key-decisions)
2. [Actions](#2-actions)
3. [Approach](#3-approach)
4. [Resource naming](#4-resource-naming)
5. [Subnet migration](#5-subnet-migration)
6. [Upgrading resources and amending resource types](#6-upgrading-resources-and-amending-resource-types)
7. [Identity and network access](#7-identity-and-network-access)
8. [Open questions](#8-open-questions)
9. [Follow-up work](#9-follow-up-work)

## 1. Key decisions

| # | Area | Decision | Status |
|---|---|---|---|
| 1 | Naming | Pattern `{sub}{type}-uks-{workload}{nn}`, e.g. `s292d01app-uks-dataapi01`. The `{nn}` instance number is only used where decision 25 says so. | Decided |
| 2 | Naming | Storage accounts drop the `01` and all hyphens, and stay within 24 characters, e.g. `s292d01stukscore`. | Decided |
| 3 | Naming | Workload names `dataapi` and `contentapi` (not `data` and `content`); shared resources use `ees`. | Decided |
| 4 | Naming | The `papi` sub-project is combined, unhyphenated, into the workload (`papiprocessor`). `sapi` is dropped (just `screener`). | Decided |
| 5 | Naming | Standard CAF/azd abbreviations (`app`, `func`, `plan`, `appi`, `st`, `sigr`, `logic`, `adf`, ...) for ELZ; the legacy set stays for existing environments. | Decided |
| 6 | Naming | Generic Front Door public site names (`-fdog`, `-fdo`, `-fdr`) get purpose names (`publicsite`). | Decided |
| 7 | Naming | Child resources of the App Gateway use short `{workload}-{purpose}` names without the subscription prefix, e.g. `publicsite-backend`. Request routing rules and path rules end in `-rule`, e.g. `papi-routing-rule`, `papidocs-path-rule`. | Decided |
| 8 | Naming | Key Vault certificates and secrets drop the `ees-` prefix and any subscription prefixes. The Key Vault is per-environment and EES-only, so the prefixes add nothing. They're named `{workload}-{purpose}`, using the same workload names as the resources (`publicsite`, `contentapi`, `dataapi`, `papi`, `papiprocessor`, ...), with `connection-string` always hyphenated. For example `publicsite-certificate`, `publicsite-basic-auth-username`, `admin-sql-password`, `publisher-publicdatadb-connection-string`. | Decided |
| 9 | Naming | Storage connection-string secrets are named `{workload}-storage-connection-string`, e.g. `core-storage-connection-string`, `importer-storage-connection-string`. | Decided |
| 10 | Mechanism | One `elz` flag selects ELZ names. It's a pipeline variable in each environment's variable group (`elz: true`); unset or anything else means legacy. Pipelines then use the param files in `parameters/elz/`, which extend the legacy ones (Bicep `extends`), set `param elz = true` and override only what differs. | Decided |
| 11 | Mechanism | Every pipeline reads names from `infrastructure/templates/common/resource-names.bicep` (Bicep) or `get-resource-names.yml` (YAML). | Done (stack #7429) |
| 12 | Subscriptions | Remove all subscription-ID allow-lists (e.g. `frontDoor.bicep`'s `@allowed` list). s180 is no longer a target. | Decided |
| 13 | Resource groups | The Container Registry and the pipeline agent VM scale sets move into their own resource group, separate from the Dev resource group but connected to it. Its name and network connection (e.g. VNet peering) are still to be decided. | Decided |
| 14 | Hosting | Linux for every App Service and Function App. | Decided |
| 15 | Network | Private endpoints instead of service endpoints, with public network access off from the start. | Decided |
| 16 | Identity | Managed identity instead of connection strings and keys wherever the service supports it. | Decided |
| 17 | Monitoring | Every app sends telemetry to Application Insights by connection string; the remaining instrumentation-key uses are removed as part of the move. See [section 6.6](#66-monitoring). | Decided |
| 18 | Hostnames | The Content API uses `cont.` in every environment: `cont.dev.`, `cont.test.`, `cont.pre-production.` and `cont.` for Prod, all on `explore-education-statistics.service.gov.uk`. Only Pre-Prod uses it today. See [section 6.11](#611-content-api-hostnames). | Decided |
| 19 | Redundancy | No failover as the service doesn't demand it. Geo-redundant copies and backups that need no secondary deployment, and zone redundancy within UK South only where it costs little, in Pre-Prod and Prod. No active secondary region. See [section 6.10](#610-redundancy). | Decided |
| 20 | Durable Functions | The data processor keeps the Azure Storage backend for orchestration state, but connects to it with its managed identity instead of connection strings. See [section 6.12](#612-data-processor-durable-functions-storage). | Decided |
| 21 | Network | Subnet layout: one integration subnet per App Service plan sized to twice its maximum instance count, one shared private endpoint subnet, dedicated App Gateway and Container Apps subnets, in a /21 per environment. See [section 5](#5-subnet-migration). | Decided |
| 22 | Network | VNet and subnet address ranges come from the `.bicepparam` files (EES-7425). | Done |
| 23 | Network | ELZ IP ranges. Allocated so far: Dev `10.226.192.0/24`, Test `10.221.192.0/24`, Pre-Prod `10.216.192.0/25`, Prod `10.216.192.128/25`. Too small for the layout in [section 5](#5-subnet-migration); requesting a /21 per environment (a /22 is the minimum). | Blocked (platform team) |
| 24 | Secrets | Only store secrets that something reads. Storage account connection strings are only written to Key Vault when a `connectionStringSecretName` is supplied, and storage access keys are never stored. Anything that needs a key directly (e.g. Azure Files mounts) uses `listKeys()` at deploy time. Values that aren't sensitive (e.g. managed identity connection strings, internal URLs) are passed as plain values, not through Key Vault, where they can be derived by other means. | Decided |
| 25 | Naming | The `01` instance number is only used on resources we could run two of for the same purpose (apps, plans, App Insights, Key Vault, SQL and PostgreSQL servers, VNet, App Gateway, Front Door profile and endpoint, Search, Container Apps environment, Data Factory, vaults, identities, alerts). Resources named by their purpose drop it: Event Grid topics and subscriptions, backup and WAF policies, the Front Door security policy, Front Door custom domains, origin groups, origins and routes, subnets and file shares. e.g. `s292d01evgt-uks-publicationchanged`, `snet-uks-admin` (see decision 26 for the prefix). | Decided |
| 26 | Naming | Policies and child resources drop the environment prefix too, since their names only need to be unique within their parent or resource group: `{type}-uks-{purpose}`. Covers WAF policies, backup policies, the Front Door security policy, Front Door custom domains, origin groups, origins and routes, Event Grid subscriptions, subnets and file shares, e.g. `waf-uks-agwglobal`, `snet-uks-admin`, `fdfpuksees`. Event Grid topics keep the prefix because their endpoints must be unique within the region. Top-level resources keep it so they're distinguishable across environments in alerts, cost reports and Log Analytics. | Decided |
| 27 | Naming | Event Grid topics deliberately keep the environment prefix (but not `01`), e.g. `s292d01evgt-uks-publicationchanged`. A custom topic's name becomes part of its public endpoint hostname, `https://{topic}.uksouth-1.eventgrid.azure.net/api/events`, so it must be unique within the region across all of Azure. Without the prefix, every environment would try to claim the same hostname in UK South, and any other Azure customer using the name would block us. Their child Event Grid subscriptions don't need it (decision 26). | Decided |

## 2. Actions

Status: **Done** (merged), **In progress** (in review or uncommitted), **To do**, **Blocked**.

| # | Action | Status | Notes |
|---|---|---|---|
| 1 | Move the VNet and subnet address ranges into the core `.bicepparam` files | Done | EES-7425 |
| 2 | Build the shared naming module `common/resource-names.bicep`, reproducing today's names exactly | In progress | Stack #7429; first PRs merged |
| 3 | Move every pipeline (Bicep and YAML) onto the shared names, verified with identical snapshots | In progress | Stack #7429. YAML pipelines use `get-resource-names.yml` |
| 4 | Add the `elz` flag to `ResourceNamingContext`, the ELZ abbreviations (`abbreviations-elz.bicep`) and `getElzResourceNames()` | In progress | Legacy output verified unchanged. ELZ output matches [section 4](#4-resource-naming) except the interim subnets |
| 5 | Make the Robot UI tests job pick its Key Vault name from the `elz` flag | In progress | Interim Bash step; see [follow-up 1](#9-follow-up-work) |
| 6 | Pass the `elz` flag into every template's naming context, and create the `parameters/elz/` param files | In progress | All 14 templates take `param elz bool = false`. 56 ELZ param files each extend their legacy file and set `elz = true`. The new `use-elz-param-file.yml` step switches each deploy job's `$(paramFile)` to `elz/` when the variable group's `elz` is `true`. Verified by snapshot: legacy output unchanged; all 56 ELZ param files deploy-snapshot with ELZ names |
| 7 | Let `get-resource-names.yml` take the `elz` flag | In progress | Sets `RESOURCE_NAMES_ELZ=true` only when the variable group's `elz` is exactly `true` |
| 8 | Add `elz: true` and `subscription` to the s292 environments' variable groups | To do | Once the environments exist |
| 9 | Send the questions in [section 8](#8-open-questions) to the platform team | To do | Draft message written |
| 10 | Get ELZ IP ranges big enough for the [section 5](#5-subnet-migration) layout: a /21 per environment | Blocked | The /24s (Dev, Test) and /25s (Pre-Prod, Prod) allocated so far are too small. Reply sent to the platform team asking for /21s |
| 10a | Put the ELZ IP ranges into the `parameters/elz/` param files, using the [section 5](#5-subnet-migration) layout | To do | Once action 10 is unblocked |
| 11 | Apply naming decisions 7-9 (App Gateway child names, Key Vault certificate and secret names, storage connection-string secret names) in `getElzResourceNames()` | In progress | Core, public, Importer, Notifier, Publisher and Screener storage modules now take their connection-string secret name from `resourceNames`. Legacy output verified unchanged |
| 11a | Move the Key Vault secret names that are still hardcoded into `resourceNames`, so they follow the `elz` flag | To do | About 15 in Bicep (e.g. `ees-alerts-slackconfig`, `ees-sql-admin-password`, `ees-publicapi-app-gateway-fuapi-header`) and the Robot tests and Public API pipeline YAML |
| 11b | Stop storing unused secrets: `storage-account.bicep` takes `connectionStringSecretName` (was `connectionStringSecretNameOverride`), stores the connection string only when it's supplied, and never stores access keys | In progress | Removes 16 access-key secrets and 3 unused connection strings (Public API data, Analytics and screener logs storage) per environment. Verified by snapshot: nothing else changes |
| 11c | Stop using Key Vault for values that aren't secret where they can be derived by other means: the three PostgreSQL connection strings (managed identity, no password) are built with `publicDataDbConnectionString()` in `common/functions.bicep`, and Admin reads the Public API's private URL from the Container App itself | In progress | Removes 4 secrets per environment, and `publisher.publicDataDbConnectionString` and `publicApiContainerAppPrivateUrl` from `resourceNames`. The data processor's pipeline sets its connection string from the new `dataProcessorPsqlConnectionString` output. Verified by snapshot: the apps' connection strings and the Admin private URL match what the secrets held |
| 12 | Change the `ResourceNames` subnets and the VNet module to the [section 5](#5-subnet-migration) layout (15 subnets, one shared `snet-uks-pep`) | To do | The layout is decided; doesn't need the IP ranges |
| 13 | Move names still built inside modules into `resourceNames`: private endpoints, alerts, the private DNS VNet link, the resource group | To do | They won't follow the `elz` flag until then |
| 14 | Remove the subscription allow-lists, e.g. the `@allowed` list in `frontDoor.bicep` | To do | Decision 12 |
| 15 | Create the shared tooling resource group for the Container Registry and agent VMSS, and agree its name and network connection | To do | Decision 13; open question 6 |
| 16 | Add the Azure CLI to `vmss-agent-bootstrap.sh` and reimage the agent scale sets | To do | Unblocks follow-up 1 |
| 17 | Move the Windows apps to Linux: publish flags, `linuxFxVersion`, app settings, testing | Done | [Section 6.2](#62-windows--linux). Done under the `elz` flag (EES-7802). Still to test on Linux: the Importer's `libmagic` dependency and case-sensitive file paths. |
| 18 | Replace the remaining App Insights instrumentation keys with connection strings | To do | [Section 6.6](#66-monitoring) |
| 19 | Connect the data processor's Durable Functions storage with managed identity | To do | [Section 6.12](#612-data-processor-durable-functions-storage) |
| 20 | Apply the redundancy settings, and decide which environments get them | In progress | [Section 6.10](#610-redundancy). Storage done in the `parameters/elz` files (EES-7803): SKUs per [section 6.4](#64-storage-redundancy-and-access), with no geo copy in Dev and Test, and the file share backup policy on the vault tier. Vaults done: both geo-redundant in every environment, and cross-region restore turned on for the Backup vault (the Recovery Services vault already had it). App Service plans and Container Apps done: zone redundancy on the Admin, Content API, Data API and Public Site plans and the Container Apps environment, in Pre-Prod and Prod only. SQL, PostgreSQL, Search and the function app plans are deliberately left without zone redundancy. |
| 21 | Move the Content API to `cont.` hostnames, keeping `content.` on Prod for a transition period | To do | [Section 6.11](#611-content-api-hostnames) |
| 22 | Identity and access groundwork: user-assigned identities per app, SQL Entra-only, Key Vault RBAC, split key-based storage from identity-only | To do | [Section 7](#7-identity-and-network-access) |
| 23 | Tidy-ups: retire `infrastructure/infrastructure-pipeline.yml`, delete the unused Basic ACR module, update preview API versions | To do | [Section 6.9](#69-api-versions-and-leftovers) |

## 3. Approach

1. **Shared naming module, legacy names first.** `resource-names.bicep` reproduces today's names exactly, and each pipeline was moved onto it with identical before-and-after snapshots. Done.
2. **Add the ELZ names behind the flag.** Add an `elz` field to `ResourceNamingContext`, an ELZ abbreviations set, and a `getElzResourceNames()` function returning the same `ResourceNames` type. Verify that legacy output is unchanged, then review the generated ELZ names against [section 4](#4-resource-naming).
3. **Wire the flag into the pipelines** and add the `parameters/elz/` files.
4. **Apply the type and SKU changes** in [section 6](#6-upgrading-resources-and-amending-resource-types) as part of building the new environments, rather than as in-place upgrades.

## 4. Resource naming

One row per entry in `resourceNames`, generated from `getResourceNamesForEnvironment()` on the flag branch. "NEW" marks entries added since the first version of this table. Other environments swap `d01` for `t01`, `p01`, etc. Subnets are in [section 5](#5-subnet-migration).

Names that aren't in `resourceNames` yet, so aren't listed here: the private DNS VNet link (`s101d01-vnet-ees-database-vnetlink`), the resource group (`s101d01-rg-ees`), and the pipeline runners' VNet and subnet.

### Shared / core

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `vnet.vnet` | Virtual Network | `s101d01-vnet-ees` | `s292d01vnet-uks-ees01` |  |
| `acr.serverName` | Container Registry (Dev only) | `eesacr` | `s292d01cruksees01` | No hyphens allowed. Moves to the shared tooling resource group with the agent VMSS (decision 13) |
| `applicationGateway.applicationGateway` | Application Gateway | `s101d01-ees-agw-01` | `s292d01agw-uks-ees01` |  |
| `applicationGateway.managedIdentity` | Managed Identity | `s101d01-ees-id-agw-01` | `s292d01id-uks-agw01` |  |
| `applicationGateway.publicIpAddress` | Public IP | `s101d01-ees-papi-pip` | `s292d01pip-uks-agw01` | It is the gateway's IP, not the Public API's |
| `applicationGateway.frontendIpConfig` | AGW frontend IP config (child) | `s101d01-ees-papi-public-frontend-ip-config` | `public-frontend-ip-config` | NEW. Child of the gateway: unique per gateway, so no prefix needed |
| `applicationGateway.globalWafPolicy` | App Gateway WAF Policy | `s101d01-ees-agw-01-global-afwp` | `waf-uks-agwglobal` | `afwp` is the Azure Firewall abbreviation |
| `applicationGateway.sites.publicSite.backend` | AGW backend pool (child) | `s101d01-as-ees-public-site-backend` | `publicsite-backend` | NEW. Child of the gateway |
| `applicationGateway.sites.publicSite.route` | AGW routing rule (child) | `s101d01-as-ees-public-site-route` | `publicsite-routing-rule` | Decision 7: App Gateway rules end in `-rule` |
| `applicationGateway.sites.publicSite.site` | AGW listener/site (child) | `s101d01-as-ees-public-site-site` | `publicsite-site` | NEW. Child of the gateway |
| `applicationGateway.sites.publicApi.backend` | AGW backend pool (child) | `s101d01-ees-papi-ca-api` | `papi-backend` | Child of the gateway; today it reuses the container app's name |
| `applicationGateway.sites.publicApi.route` | AGW routing rule (child) | `s101d01-ees-papi` | `papi-routing-rule` | Decision 7: App Gateway rules end in `-rule` |
| `applicationGateway.sites.publicApi.site` | AGW listener/site (child) | `s101d01-ees-papi` | `papi-site` | NEW. Child of the gateway |
| `applicationGateway.sites.publicApi.wafPolicy` | App Gateway WAF Policy | `s101d01-ees-agw-01-public-api-afwp` | `waf-uks-agwpapi` | `afwp` is the Azure Firewall abbreviation |
| `applicationGateway.sites.publicApi.rewriteSet` | AGW rewrite rule set (child) | `s101d01-ees-papi-docs-rewrites` | `papidocs-rewrites` | Decision 7: named after the docs site it rewrites for |
| `applicationGateway.sites.publicApiDocs.backend` | AGW backend pool (child) | `s101d01-ees-papi-stapp-docs` | `papidocs-backend` | Child of the gateway; today it reuses the static web app's name |
| `applicationGateway.sites.publicApiDocs.pathRules.backend` | AGW path rule (child) | `docs-backend` | `papidocs-path-rule` | Decision 7: workload-first, like the other App Gateway child names |
| `applicationGateway.sites.publicApiDocs.pathRules.rootRedirect` | AGW path rule (child) | `docs-root-redirect` | `papidocs-root-redirect-rule` | Decision 7: App Gateway rules end in `-rule` |
| `backups.backupVault` | Backup Vault | `s101d01-ees-bvault` | `s292d01bvault-uks-ees01` |  |
| `backups.backupVaultBlobsPolicy` | Backup Vault Policy | `s101d01-ees-blob-bkpol` | `bkpol-uks-blob` |  |
| `backups.backupVaultPsqlFlexibleServerPolicy` | Backup Vault Policy | `s101d01-ees-psql-flexible-server-bkpol` | `bkpol-uks-psql` | Third PostgreSQL spelling removed |
| `backups.recoveryServicesVault` | Recovery Services Vault | `s101d01-ees-rsv` | `s292d01rsv-uks-ees01` |  |
| `containerAppEnvironment` | Container App Environment | `s101d01-ees-cae-01` | `s292d01cae-uks-ees01` |  |
| `dataFactory` | Data Factory | `s101d01-df-ees-release` | `s292d01adf-uks-ees01` | `df`→`adf` |
| `frontDoor.frontDoorName` | Front Door Profile | `s101d01-ees-afd` | `s292d01afd-uks-ees01` | ⚠ global resource, so `uks` is misleading |
| `frontDoor.defaultEndpoint.endpointName` | Front Door Endpoint | `s101d01-ees-fde` | `s292d01fde-uks-ees01` | Endpoint names are globally unique hostnames |
| `frontDoor.wafPolicy` | Front Door WAF Policy | `s101d01eesafdwafp` | `fdfpuksees` | Alphanumeric only; `wafp`→`fdfp` |
| `frontDoor.wafSecurityPolicy` | Front Door Security Policy | `s101d01eesafdwafsp` | `afdsp-uks-ees` | Hyphens are allowed here |
| `frontDoor.publicSite.customDomain` | Front Door Custom Domain | `s101d01-ees-public-site-afd-domain` | `afdd-uks-publicsite` |  |
| `frontDoor.publicSite.originGroup` | Front Door Origin Group | `s101d01-ees-fdog` | `fdog-uks-publicsite` | Says which site (TODO EES-7502) |
| `frontDoor.publicSite.origin` | Front Door Origin | `s101d01-ees-fdo` | `fdo-uks-publicsite` | Says which site (TODO EES-7502) |
| `frontDoor.publicSite.route` | Front Door Route | `s101d01-ees-fdr` | `fdr-uks-publicsite` | Says which site (TODO EES-7502) |
| `frontDoor.contentApi.customDomain` | Front Door Custom Domain | `s101d01-ees-content-afd-domain` | `afdd-uks-contentapi` |  |
| `frontDoor.contentApi.originGroup` | Front Door Origin Group | `s101d01-ees-content-fdog` | `fdog-uks-contentapi` |  |
| `frontDoor.contentApi.origin` | Front Door Origin | `s101d01-ees-content-fdo` | `fdo-uks-contentapi` |  |
| `frontDoor.contentApi.route` | Front Door Route | `s101d01-ees-content-fdr` | `fdr-uks-contentapi` |  |
| `frontDoor.contentApi.allFilesCacheRoute` | Front Door Route | `s101d01-ees-content-all-files-cache-fdr` | `fdr-uks-contentapiallfilescache` |  |
| `storageAccounts.core` | Storage Account | `s101d01saeescore` (Test/Pre-Prod `s101t01storageeescore`) | `s292d01stukscore` | Test/Pre-Prod `storage` special case removed |
| `storageAccounts.public` | Storage Account | `s101d01saeespublic` | `s292d01stukspublic` |  |
| `storageAccounts.logging` | Storage Account | `s101d01saeeslogging` | `s292d01stukslogging` |  |
| `keyVault.keyVault` | Key Vault | `s101d01-kv-ees-01` | `s292d01kv-uks-ees01` |  |
| `keyVault.keys.dataProtection` | Key Vault key | `ees-dataprotection-key` | `ees-dataprotection-key` | NEW. Unchanged: the vault is per-environment |
| `keyVault.certificates.publicSite` | Key Vault certificate | `s101d01-as-ees-public-site-certificate` | `publicsite-certificate` | Decision 8: no `ees-` or subscription prefix |
| `keyVault.certificates.contentApiFrontDoor` | Key Vault certificate | `s101d01-as-ees-content-afd-certificate` | `contentapi-afd-certificate` | Decision 8: no `ees-` or subscription prefix |
| `keyVault.certificates.publicApiAppGateway` | Key Vault certificate | `s101d01-ees-papi-certificate` | `papi-agw-certificate` | Decision 8: no `ees-` or subscription prefix |
| `alertsGroup` | Action Group | `s101d01-ag-ees-alertedusers` | `s292d01ag-uks-alertedusers01` |  |
| `alertsLogicApp` | Logic App | `s101d01-la-ees-slackwebhook` | `s292d01logic-uks-alerts01` | `la`→`logic` (or drop it with the Slack webhooks, EES-7608) |
| `databases.coreSqlServer` | SQL Server | `s101d01-sqlsvr-ees-01` | `s292d01sql-uks-core01` | Named by purpose |
| `databases.publicSqlServer` | SQL Server | `s101d01-sqlsvr-ees-02` | `s292d01sql-uks-public01` | Named by purpose |
| `databases.contentDb` | SQL Database | `content` | `content` | NEW. Unchanged |
| `databases.statisticsDb` | SQL Database | `statistics` | `statistics` | NEW. Unchanged |
| `databases.postgreSqlFlexibleServer` | PostgreSQL Flexible Server | `s101d01-ees-psql-flexibleserver` | `s292d01psql-uks-ees01` | `-flexibleserver` suffix removed (TODO EES-7502) |
| `eventGrid.topics.publicationChanged` | Event Grid Topic | `s101d01-ees-evgt-publication-changed` | `s292d01evgt-uks-publicationchanged` |  |
| `eventGrid.topics.releaseChanged` | Event Grid Topic | `s101d01-ees-evgt-release-changed` | `s292d01evgt-uks-releasechanged` |  |
| `eventGrid.topics.releaseVersionChanged` | Event Grid Topic | `s101d01-ees-evgt-release-version-changed` | `s292d01evgt-uks-releaseversionchanged` |  |
| `eventGrid.topics.themeChanged` | Event Grid Topic | `s101d01-ees-evgt-theme-changed` | `s292d01evgt-uks-themechanged` |  |
| `logAnalyticsWorkspace` | Log Analytics Workspace | `s101d01-ees-log` | `s292d01log-uks-ees01` |  |

### Main apps

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `admin.appService` | App Service | `s101d01-as-ees-admin` | `s292d01app-uks-admin01` |  |
| `admin.appServicePlan` | App Service Plan | `s101d01-asp-ees-admin` | `s292d01plan-uks-admin01` |  |
| `admin.appInsights` | Application Insights | `s101d01-ai-ees-admin` | `s292d01appi-uks-admin01` |  |
| `admin.signalRName` | SignalR | `s101d01-sr-ees-admin` | `s292d01sigr-uks-admin01` |  |
| `contentApi.appService` | App Service | `s101d01-as-ees-content` | `s292d01app-uks-contentapi01` |  |
| `contentApi.appServicePlan` | App Service Plan | `s101d01-asp-ees-content` | `s292d01plan-uks-contentapi01` |  |
| `contentApi.appInsights` | Application Insights | `s101d01-ai-ees-content` | `s292d01appi-uks-contentapi01` |  |
| `dataApi.appService` | App Service | `s101d01-as-ees-data` | `s292d01app-uks-dataapi01` |  |
| `dataApi.appServicePlan` | App Service Plan | `s101d01-asp-ees-data` | `s292d01plan-uks-dataapi01` |  |
| `dataApi.appInsights` | Application Insights | `s101d01-ai-ees-data` | `s292d01appi-uks-dataapi01` |  |
| `importer.functionApp` | Function App | `s101d01-fa-ees-importer` | `s292d01func-uks-importer01` |  |
| `importer.appServicePlan` | App Service Plan | `s101d01-asp-ees-importer` | `s292d01plan-uks-importer01` |  |
| `importer.appInsights` | Application Insights | `s101d01-ai-ees-importer` | `s292d01appi-uks-importer01` |  |
| `importer.storageAccount` | Storage Account | `s101d01eessaimporter` | `s292d01stuksimporter` |  |
| `notifier.functionApp` | Function App | `s101d01-fa-ees-notify` | `s292d01func-uks-notifier01` | `notify`→`notifier` |
| `notifier.appServicePlan` | App Service Plan | `s101d01-asp-ees-notify` | `s292d01plan-uks-notifier01` |  |
| `notifier.appInsights` | Application Insights | `s101d01-ai-ees-notify` | `s292d01appi-uks-notifier01` |  |
| `notifier.storageAccount` | Storage Account | `s101d01saeesnotify` (Test/Pre-Prod `s101t01storageeesnotify`) | `s292d01stuksnotifier` | Test/Pre-Prod `storage` special case removed |
| `publicSite.appService` | App Service | `s101d01-as-ees-public-site` | `s292d01app-uks-publicsite01` |  |
| `publicSite.appServicePlan` | App Service Plan | `s101d01-asp-ees-public-site` | `s292d01plan-uks-publicsite01` |  |
| `publicSite.appInsights` | Application Insights | `s101d01-ai-ees-public-site` | `s292d01appi-uks-publicsite01` |  |
| `publisher.functionApp` | Function App | `s101d01-fa-ees-publisher` | `s292d01func-uks-publisher01` |  |
| `publisher.appServicePlan` | App Service Plan | `s101d01-asp-ees-publisher` | `s292d01plan-uks-publisher01` |  |
| `publisher.appInsights` | Application Insights | `s101d01-ai-ees-publisher` | `s292d01appi-uks-publisher01` |  |
| `publisher.storageAccount` | Storage Account | `s101d01saeespublisher` | `s292d01stukspublisher` |  |

### Public API (`papi`)

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `publicApi.apiApp` | Container App | `s101d01-ees-papi-ca-api` | `s292d01ca-uks-papiapi01` |  |
| `publicApi.apiAppIdentity` | Managed Identity | `s101d01-ees-papi-id-ca-api` | `s292d01id-uks-papiapi01` | Embedded `ca` removed |
| `publicApi.appInsights` | Application Insights | `s101d01-ees-papi-ai` | `s292d01appi-uks-papi01` |  |
| `publicApi.docsApp` | Static Web App | `s101d01-ees-papi-stapp-docs` | `s292d01stapp-uks-papidocs01` |  |
| `publicApi.processor.functionApp` | Function App | `s101d01-ees-papi-fa-processor` | `s292d01func-uks-papiprocessor01` |  |
| `publicApi.processor.appServicePlan` | App Service Plan | `s101d01-ees-papi-fa-processor` | `s292d01plan-uks-papiprocessor01` | No longer reuses the function app's name (TODO EES-7502) |
| `publicApi.processor.identity` | Managed Identity | `s101d01-ees-papi-id-fa-processor` | `s292d01id-uks-papiprocessor01` | Embedded `fa` removed |
| `publicApi.processor.storageAccountsPrefix` | Storage Account prefix (→ `mg`, `s1`, `s2`) | `s101d01eessaprocessor` | `s292d01stukspapiproc` | `papiprocessor` + `mg` would be 27 chars, over the 24 limit, so shortened to `papiproc` (22 with suffix) |
| `publicApi.storage.storageAccount` | Storage Account | `s101d01eespapisa` | `s292d01stukspapi` |  |
| `publicApi.storage.fileShare` | File Share | `s101d01-ees-papi-share-data` | `share-uks-papidata` |  |

### Screener (`sapi` dropped)

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `screener.functionApp` | Function App | `s101d01-ees-sapi-fa-screener` | `s292d01func-uks-screener01` | `sapi` dropped |
| `screener.appServicePlan` | App Service Plan | `s101d01-ees-sapi-fa-screener` | `s292d01plan-uks-screener01` | No longer reuses the function app's name (TODO EES-7502) |
| `screener.appInsights` | Application Insights | `s101d01-ees-sapi-ai-screener` | `s292d01appi-uks-screener01` | Last table had the current name wrong: it does include `sapi` |
| `screener.functionStorageAccount` | Storage Account | `s101d01eessapisafn` | `s292d01stuksscreenerfn` |  |
| `screener.logsStorageAccount` | Storage Account | `s101d01eessapisalogs` | `s292d01stuksscreenerlogs` | Exactly 24 chars |
| `screener.logsFileShare` | File Share | `s101d01-ees-sapi-fa-screener-logs-share` | `share-uks-screenerlogs` |  |

### Search

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `nlSearch.functionApp` | Function App | `s101d01-ees-fa-nlsearch` | `s292d01func-uks-nlsearch01` |  |
| `nlSearch.appServicePlan` | App Service Plan | `s101d01-ees-asp-nlsearch` | `s292d01plan-uks-nlsearch01` |  |
| `nlSearch.functionStorageAccount` | Storage Account | `s101d01eessanlsearchfn` | `s292d01stuksnlsearchfn` |  |
| `search.service` | Search Service | `s101d01-ees-srch` | `s292d01srch-uks-ees01` |  |
| `search.storageAccount` | Storage Account | `s101d01eessasearch` | `s292d01stukssearch` |  |
| `search.appInsights` | Application Insights | `s101d01-ees-ai-search` | `s292d01appi-uks-search01` | Shared by nlSearch and searchDocs |
| `search.searchDocsEventSubscriptions.publicationArchived` | Event Grid Subscription | `s101d01-ees-evgs-publication-archived` | `evgs-uks-publicationarchived` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.publicationChanged` | Event Grid Subscription | `s101d01-ees-evgs-publication-changed` | `evgs-uks-publicationchanged` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.publicationDeleted` | Event Grid Subscription | `s101d01-ees-evgs-publication-deleted` | `evgs-uks-publicationdeleted` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.publicationLatestPublishedReleaseReordered` | Event Grid Subscription | `s101d01-ees-evgs-publication-latest-published-release-reordered` | `evgs-uks-publicationlatestpublishedreleasereordered` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.publicationRestored` | Event Grid Subscription | `s101d01-ees-evgs-publication-restored` | `evgs-uks-publicationrestored` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.releaseSlugChanged` | Event Grid Subscription | `s101d01-ees-evgs-release-slug-changed` | `evgs-uks-releaseslugchanged` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.releaseVersionPublished` | Event Grid Subscription | `s101d01-ees-evgs-release-version-published` | `evgs-uks-releaseversionpublished` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.themeChanged` | Event Grid Subscription | `s101d01-ees-evgs-theme-changed` | `evgs-uks-themechanged` | NEW (last table only had a `{topic}` placeholder) |
| `searchDocs.functionApp` | Function App | `s101d01-ees-fa-searchdocs` | `s292d01func-uks-searchdocs01` |  |
| `searchDocs.appServicePlan` | App Service Plan | `s101d01-ees-asp-searchdocs` | `s292d01plan-uks-searchdocs01` |  |
| `searchDocs.functionStorageAccount` | Storage Account | `s101d01eessasearchdocsfn` | `s292d01stukssearchdocsfn` | Exactly 24 chars |

### Analytics

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `analytics.functionApp` | Function App | `s101d01-ees-fa-analytics` | `s292d01func-uks-analytics01` |  |
| `analytics.appServicePlan` | App Service Plan | `s101d01-ees-asp-analytics` | `s292d01plan-uks-analytics01` |  |
| `analytics.appInsights` | Application Insights | `s101d01-ees-ai-analytics` | `s292d01appi-uks-analytics01` |  |
| `analytics.functionStorageAccount` | Storage Account | `s101d01eessaanlytfa` | `s292d01stuksanalyticsfn` | `fa`→`fn`, matching the other function storage accounts |
| `analytics.storage.storageAccountName` | Storage Account | `s101d01eessaanlyt` | `s292d01stuksanalytics` |  |
| `analytics.storage.fileShareName` | File Share | `s101d01-ees-share-anlyt` | `share-uks-analytics` | Child of the storage account, so the prefix is optional |

### Key Vault secrets named after a storage account today

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `keyVault.secrets.admin.screenerStorageAccountConnectionString` | Key Vault secret | `s101d01eessapisafn-connection-string` | `screener-storage-connection-string` | Decision 9 |
| `keyVault.secrets.importerStorageAccountConnectionString` | Key Vault secret | `s101d01eessaimporter-connection-string` | `importer-storage-connection-string` | Decision 9 |
| `keyVault.secrets.notifierStorageAccountConnectionString` | Key Vault secret | `s101d01saeesnotify-connection-string` (Test/Pre-Prod `s101t01storageeesnotify-connection-string`) | `notifier-storage-connection-string` | Decision 9 |
| `keyVault.secrets.publisherStorageAccountConnectionString` | Key Vault secret | `s101d01saeespublisher-connection-string` | `publisher-storage-connection-string` | Decision 9 |
| `keyVault.secrets.analyticsStorageAccountConnectionString` | Key Vault secret | `s101d01eessaanlytfa-connection-string` | `analytics-storage-connection-string` | NEW. Decision 9; read by the Function App for its content share |
| `keyVault.secrets.nlSearchStorageAccountConnectionString` | Key Vault secret | `s101d01eessanlsearchfn-connection-string` | `nlsearch-storage-connection-string` | NEW. Decision 9; read by the Function App for its content share |
| `keyVault.secrets.searchDocsStorageAccountConnectionString` | Key Vault secret | `s101d01eessasearchdocsfn-connection-string` | `searchdocs-storage-connection-string` | NEW. Decision 9; read by the Function App for its content share |
| `keyVault.secrets.searchStorageAccountConnectionString` | Key Vault secret | `s101d01eessasearch-connection-string` | `search-storage-connection-string` | NEW. Decision 9; read by the Search Docs Function App |

### Other Key Vault secrets

Decision 8 drops the `ees-` prefix and puts the workload name first; decision 9 renames the core and public storage connection strings. `DOCKER-REGISTRY-*` keep their names. The data protection key (`keyVault.keys.dataProtection`) is a Key Vault key rather than a secret, so it keeps `ees-dataprotection-key`.

| `resourceNames` path | Current | ELZ |
|---|---|---|
| `keyVault.secrets.acr.dockerPullUsername` | `DOCKER-REGISTRY-SERVER-USERNAME` | `DOCKER-REGISTRY-SERVER-USERNAME` |
| `keyVault.secrets.acr.dockerPullPassword` | `DOCKER-REGISTRY-SERVER-PASSWORD` | `DOCKER-REGISTRY-SERVER-PASSWORD` |
| `keyVault.secrets.acr.dockerRegistryServerDomain` | `DOCKER-REGISTRY-SERVER-DOMAIN` | `DOCKER-REGISTRY-SERVER-DOMAIN` |
| `keyVault.secrets.admin.adminGovUkNotifyApiKey` | `ees-admin-govuknotify-api-key` | `admin-govuknotify-api-key` |
| `keyVault.secrets.admin.adminSignalrConnectionString` | `ees-signalr-admin-connectionstring` | `admin-signalr-connection-string` |
| `keyVault.secrets.admin.databaseUserPassword` | `ees-sql-password-admin` | `admin-sql-password` |
| `keyVault.secrets.admin.openIdConnectClientId` | `ees-openidconnect-clientid` | `admin-openidconnect-client-id` |
| `keyVault.secrets.admin.openIdConnectAuthority` | `ees-openidconnect-authority` | `admin-openidconnect-authority` |
| `keyVault.secrets.admin.openIdConnectValidAudience` | `ees-openidconnect-valid-audience` | `admin-openidconnect-valid-audience` |
| `keyVault.secrets.admin.openIdConnectValidIssuers` | `ees-openidconnect-valid-issuers` | `admin-openidconnect-valid-issuers` |
| `keyVault.secrets.admin.openIdConnectFullyQualifiedScopeName` | `ees-openidconnect-fully-qualified-scope-name` | `admin-openidconnect-fully-qualified-scope-name` |
| `keyVault.secrets.contentApi.databaseUserPassword` | `ees-sql-public-password-content` | `contentapi-sql-password` |
| `keyVault.secrets.dataApi.databaseUserPassword` | `ees-sql-public-password-data` | `dataapi-sql-password` |
| `keyVault.secrets.importer.databaseUserPassword` | `ees-sql-password-importer` | `importer-sql-password` |
| `keyVault.secrets.notifier.databaseUserPassword` | `ees-sql-password-notifier` | `notifier-sql-password` |
| `keyVault.secrets.notifier.govUkNotifyApiKey` | `ees-notifier-govuknotify-api-key` | `notifier-govuknotify-api-key` |
| `keyVault.secrets.notifier.tokenSecretKey` | `ees-notifier-token-secret-key` | `notifier-token-secret-key` |
| `keyVault.secrets.publicApi.apiAppRegistrationClientId` | `ees-public-api-app-registration-client-id` | `papi-app-registration-client-id` |
| `keyVault.secrets.publicApi.dataProcessorAppRegistrationClientId` | `ees-public-api-data-processor-app-registration-client-id` | `papiprocessor-app-registration-client-id` |
| `keyVault.secrets.publicSite.basicAuthUsername` | `ees-public-site-basic-auth-username` | `publicsite-basic-auth-username` |
| `keyVault.secrets.publicSite.basicAuthPassword` | `ees-public-site-basic-auth-password` | `publicsite-basic-auth-password` |
| `keyVault.secrets.publisher.databaseUserPassword` | `ees-sql-password-publisher` | `publisher-sql-password` |
| `keyVault.secrets.publisher.notifyApiKey` | `ees-publisher-govuknotify-api-key` | `publisher-govuknotify-api-key` |
| `keyVault.secrets.screener.appRegistrationClientId` | `ees-screener-app-registration-client-id` | `screener-app-registration-client-id` |
| `keyVault.secrets.bauEmail` | `ees-bau-email` | `bau-email` |
| `keyVault.secrets.coreStorageAccountConnectionString` | `ees-storage-core` | `core-storage-connection-string` |
| `keyVault.secrets.publicStorageAccountConnectionString` | `ees-storage-public` | `public-storage-connection-string` |

## 5. Subnet migration

**Layout: Decided** ([decision 21](#1-key-decisions)). **Address ranges: Blocked**. The ranges allocated so far are too small for it ([decision 23](#1-key-decisions)).

- **One integration subnet per App Service plan**, delegated to `Microsoft.Web/serverFarms` and holding nothing else, so NSG rules can tell the apps apart. Each is sized to twice the app's maximum instance count, because changing a plan's size briefly doubles its instances †:
  - /26 (59 usable, up to 29 instances) for the apps capped at 15 instances: Content API, Data API and NL Search;
  - /27 (27 usable, up to 13 instances) for the rest, which are capped at 2 or 5.
- **One shared private endpoint subnet**, `snet-uks-pep`, replaces the 10 per-workload private endpoint subnets. Set `privateEndpointNetworkPolicies` on it and put each private endpoint in an Application Security Group, so NSG rules can still target individual endpoints.
- **Dedicated subnets** for the App Gateway (/26, autoscale capped at 10) and the Container Apps environment (/24; it can't be resized once created).
- **A /21 per environment.** The subnets use about half of it, and the rest is one continuous spare range for growth.

† From Azure's App Service networking guidance as we understand it; check against current docs.

### Layout

Example using `10.226.0.0/21`. Each environment uses the same layout from the start of its own range. The maximum instance counts are the autoscale caps we intend to set.

| Subnet | Resource type | CIDR | Start | End | Usable | Peak use | Max instances |
|---|---|---|---|---|---|---|---|
| `snet-uks-cae` | Container Apps environment (Public API) | `10.226.0.0/24` | 10.226.0.0 | 10.226.0.255 | 251 | All of it (assumed) | n/a: scales by node and replica within the subnet |
| `snet-uks-pep` | Private endpoints: SQL Server, PostgreSQL, Storage Accounts, Key Vault, Event Grid, AI Search, SignalR, Function Apps (inbound) | `10.226.1.0/24` | 10.226.1.0 | 10.226.1.255 | 251 | 38 today (about 60 expected in ELZ) | 251 endpoints |
| `snet-uks-agw` | Application Gateway v2 | `10.226.2.0/26` | 10.226.2.0 | 10.226.2.63 | 59 | 10 (autoscale max) | 59 |
| `snet-uks-contentapi` | App Service | `10.226.2.64/26` | 10.226.2.64 | 10.226.2.127 | 59 | 15, 30 during scale-up | 29 |
| `snet-uks-dataapi` | App Service | `10.226.2.128/26` | 10.226.2.128 | 10.226.2.191 | 59 | 15, 30 during scale-up | 29 |
| `snet-uks-nlsearch` | Function App (Elastic Premium) | `10.226.2.192/26` | 10.226.2.192 | 10.226.2.255 | 59 | 15, 30 during scale-up | 29 |
| `snet-uks-admin` | App Service | `10.226.3.0/27` | 10.226.3.0 | 10.226.3.31 | 27 | 5, 10 during scale-up | 13 |
| `snet-uks-publicsite` | App Service (Linux container) | `10.226.3.32/27` | 10.226.3.32 | 10.226.3.63 | 27 | 5, 10 during scale-up | 13 |
| `snet-uks-screener` | Function App (Elastic Premium, container) | `10.226.3.64/27` | 10.226.3.64 | 10.226.3.95 | 27 | 5, 10 during scale-up | 13 |
| `snet-uks-papiprocessor` | Function App (Elastic Premium, Durable Functions) | `10.226.3.96/27` | 10.226.3.96 | 10.226.3.127 | 27 | 5, 10 during scale-up | 13 |
| `snet-uks-analytics` | Function App (Elastic Premium) | `10.226.3.128/27` | 10.226.3.128 | 10.226.3.159 | 27 | 2, 4 during scale-up | 13 |
| `snet-uks-searchdocs` | Function App (Elastic Premium) | `10.226.3.160/27` | 10.226.3.160 | 10.226.3.191 | 27 | 5, 10 during scale-up | 13 |
| `snet-uks-importer` | Function App (Dedicated plan) | `10.226.3.192/27` | 10.226.3.192 | 10.226.3.223 | 27 | 5, 10 during scale-up | 13 |
| `snet-uks-notifier` | Function App (Dedicated plan) | `10.226.3.224/27` | 10.226.3.224 | 10.226.3.255 | 27 | 2, 4 during scale-up | 13 |
| `snet-uks-publisher` | Function App (Dedicated plan) | `10.226.4.0/27` | 10.226.4.0 | 10.226.4.31 | 27 | 2, 4 during scale-up | 13 |
| *spare* | | | 10.226.4.32 | 10.226.7.255 | 992 addresses | | |

Totals: 1,056 of 2,048 addresses allocated to subnets. About 494 are in use in normal running and about 614 at peak, counting the 5 addresses Azure reserves in every subnet.

### Why not the ranges allocated so far

| Environment | Allocated | Addresses |
|---|---|---|
| Dev | `10.226.192.0/24` | 256 |
| Test | `10.221.192.0/24` | 256 |
| Pre-Prod | `10.216.192.0/25` | 128 |
| Prod | `10.216.192.128/25` | 128 |

Even at the smallest sizes Azure supports, the App Gateway, Container Apps, integration and private endpoint subnets need more than 256 addresses. A /22 is the practical minimum, with no room to grow. Pre-Prod and Prod, which need the most capacity, have been given the least.

### Subnets: today and in ELZ

23 subnets today become 15:

| Group | Subnets |
|---|---|
| Integration (12) | `admin`, `contentapi`, `dataapi`, `publicsite` (new), `importer`, `notifier`, `publisher`, `screener`, `papiprocessor`, `nlsearch`, `searchdocs`, `analytics` |
| Dedicated (2) | `agw`, `cae` |
| Private endpoints (1) | `pep` |

All prefixed `snet-uks-`, with no environment prefix or `01` (decisions 25 and 26). New subnets with no current equivalent, to add to `resourceNames.vnet.subnets`:

| Proposed `resourceNames` path | Type | Proposed (s292 Dev) | Notes |
|---|---|---|---|
| `vnet.subnets.publicSite` | Subnet (integration) | `snet-uks-publicsite` | The public site has no VNet integration today; it needs it once the Content API is private |
| `vnet.subnets.privateEndpoints` | Subnet (private endpoints) | `snet-uks-pep` | Replaces all 10 of today's private endpoint subnets |

### Existing subnets and where they go

Until this layout is confirmed, `getElzResourceNames()` in `resource-names.bicep` keeps today's one-to-one set of subnets under ELZ names, so the VNet templates work unchanged. The private endpoint subnets are named `snet-uks-sqlpep`, `…-psql`, `…-evgtpep`, `…-papistpep`, `…-searchstpep`, `…-analyticsstpep`, `…-screenerpep`, `…-papiprocessorpep`, `…-nlsearchpep` and `…-searchdocspep`. Moving to the layout above also means changing the `ResourceNames` type and the VNet module.

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `vnet.subnets.applicationGateway` | Subnet (dedicated) | `s101d01-ees-snet-agw-01` | `snet-uks-agw` | Kept: App Gateway needs its own subnet |
| `vnet.subnets.containerAppEnvironment` | Subnet (dedicated, `Microsoft.App/environments`) | `s101d01-ees-snet-cae-01` | `snet-uks-cae` | Kept: the CAE needs its own subnet |
| `vnet.subnets.admin` | Subnet (integration) | `s101d01-snet-ees-admin` | `snet-uks-admin` |  |
| `vnet.subnets.contentApi` | Subnet (integration) | `s101d01-snet-ees-content` | `snet-uks-contentapi` |  |
| `vnet.subnets.dataApi` | Subnet (integration) | `s101d01-snet-ees-data` | `snet-uks-dataapi` |  |
| `vnet.subnets.importer` | Subnet (integration) | `s101d01-snet-ees-importer` | `snet-uks-importer` |  |
| `vnet.subnets.notifier` | Subnet (integration) | `s101d01-snet-ees-notify` | `snet-uks-notifier` | `notify`→`notifier` |
| `vnet.subnets.publisher` | Subnet (integration) | `s101d01-snet-ees-publisher` | `snet-uks-publisher` |  |
| `vnet.subnets.screenerFunctionApp` | Subnet (integration) | `s101d01-ees-sapi-snet-fa-screener` | `snet-uks-screener` | `sapi` and `fa` dropped |
| `vnet.subnets.publicApiDataProcessor` | Subnet (integration) | `s101d01-ees-papi-snet-fa-processor` | `snet-uks-papiprocessor` | `fa` dropped |
| `vnet.subnets.nlSearchFunctionApp` | Subnet (integration) | `s101d01-ees-snet-fa-nlsearch` | `snet-uks-nlsearch` | `fa` dropped |
| `vnet.subnets.searchDocsFunctionApp` | Subnet (integration) | `s101d01-ees-snet-fa-searchdocs` | `snet-uks-searchdocs` | `fa` dropped |
| `vnet.subnets.analyticsFunctionApp` | Subnet (integration) | `s101d01-ees-snet-fa-analytics` | `snet-uks-analytics` | `fa` dropped |
| `vnet.subnets.sqlServerPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-sqlsvr-pep` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet (both SQL servers) |
| `vnet.subnets.psqlFlexibleServer` | Subnet (private endpoints) | `s101d01-ees-snet-psql-flexibleserver` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet. Not delegated today: it only holds the PostgreSQL private endpoint |
| `vnet.subnets.eventGridCustomTopicPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-evgt-pep` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet (Event Grid topics) |
| `vnet.subnets.publicApiStoragePrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-papi-snet-sa-pep` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet (Public API storage) |
| `vnet.subnets.searchStoragePrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-sa-search-pep` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet (search storage, Search service) |
| `vnet.subnets.analyticsStoragePrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-sa-anlyt-pep` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet (analytics storage) |
| `vnet.subnets.screenerStoragePrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-sapi-snet-sa-screener-pep` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet (storage and the function app's inbound endpoint) |
| `vnet.subnets.publicApiDataProcessorPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-papi-snet-fa-processor-pep` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet (storage and the function app's inbound endpoint) |
| `vnet.subnets.nlSearchFunctionAppPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-fa-nlsearch-pep` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet (storage and the function app's inbound endpoint) |
| `vnet.subnets.searchDocsFunctionAppPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-fa-searchdocs-pep` | `snet-uks-pep` | REMOVED: merged into the shared private endpoint subnet |

## 6. Upgrading resources and amending resource types

Taken from the current templates (October 2026). Building new environments is the cheapest time to change OS, tier or redundancy, because several of these can't be changed in place. Entries marked † rely on Azure retirement or support dates rather than anything in the code, so check them before acting.

### 6.1 Already current, no change needed
- **Storage:** every account is already `StorageV2`, or `FileStorage` for the Premium file shares. There are no general-purpose v1 or `BlobStorage` accounts.
- **Networking edge:** the Public IP is Standard SKU, the App Gateway is `WAF_v2`, and Front Door is `Standard_AzureFrontDoor` rather than classic.
- **Monitoring:** every Application Insights instance is workspace-based.
- **Functions:** every function app runs .NET isolated or Python on Functions `~4`. Nothing is in-process.
- **PostgreSQL:** version 16 with Entra-only authentication.

### 6.2 Windows → Linux
**Decided** ([decision 14](#1-key-decisions)). An App Service plan can't change OS in place, so these move as part of the new build.

| App | Today | Plan SKU today (Dev / Test / Pre-Prod / Prod) |
|---|---|---|
| Admin | Windows App Service | P1V2 / S1 / P1V2 / P1V2 |
| Content API | Windows App Service | P1V2 / S1 / P1V2 / P1V2 |
| Data API | Windows App Service | P1V2 / S1 / P1V2 / P1V2 |
| Importer | Windows Function App, dedicated plan | P2V2 / P1V2 / P1V2 / P2V2 |
| Notifier | Windows Function App, dedicated plan | B1 / B1 / S1 / S1 |
| Publisher | Windows Function App, dedicated plan | B1 / B1 / S1 / S1 |

Already on Linux: the Public Site (container), the Public API (Container App), and the Data Processor, Analytics, NL Search, Search Docs and Screener function apps.

Changes needed:
- **Build pipeline:** `azure-pipelines-main.yml` publishes the Data API, Content API and Admin with `--self-contained true -r win-x64` (lines 177, 193 and 372). Switch to `linux-x64`, or drop self-contained. Importer, Notifier and Publisher publish without a runtime identifier, so check what they output.
- **App settings:**
  - `WEBSITE_LOAD_CERTIFICATES: '*'` is only set on Windows ([app-service.bicep:187-189](infrastructure/templates/common/components/app-service/app-service.bicep#L187-L189)). Check whether any app reads certificates from the store; on Linux they're files under `/var/ssl`.
  - `WEBSITE_NODE_DEFAULT_VERSION` is set on every App Service ([app-service.bicep:174](infrastructure/templates/common/components/app-service/app-service.bicep#L174)) but does nothing on Linux.
- **Runtime stack:** Linux uses `linuxFxVersion` (e.g. `DOTNETCORE|10.0`) in place of `netFrameworkVersion: 'v10.0'`.
- **Code and testing:** Linux file paths are case-sensitive, so test all six apps. The Importer's `libmagic` dependency ships a Linux `.so`, but it hasn't been run on Linux.
- **Tidy-up:** the Data Processor site is `kind: 'functionapp'` while its plan is Linux ([durableFunctionApp.bicep:322](infrastructure/templates/public-api/components/durableFunctionApp.bicep#L322)). Make it `functionapp,linux`.

### 6.3 App Service plan tiers
**Proposed.**
- **Move PremiumV2 plans to PremiumV3.** Every App Service plan today is on Pv2, Basic or Standard, and the SKU type only allows Basic, Standard, PremiumV2 and ElasticPremium ([app-service-plan/types.bicep](infrastructure/templates/common/components/app-service-plan/types.bicep)). Pv3 gives more memory per core and usually costs less for the same capacity † (e.g. P0v3/P1v3 against P1V2/P2V2). Compare current pricing before choosing sizes, and consider Pv4 where it's offered †.
- **Consider Flex Consumption for Notifier and Publisher** once they're on Linux. They run on B1/S1 dedicated plans today. Flex Consumption supports VNet integration and identity-based storage, but has no deployment slots. Only worth it if slot swapping isn't needed for them.
- **Keep Elastic Premium (EP1, EP3 for Screener in Prod)** for the Linux function apps.

### 6.4 Storage redundancy and access
**Decided** ([decision 19](#1-key-decisions)). Redundancy and kind are chosen at creation, and changing them later generally means migrating data.

| Account | Today | ELZ |
|---|---|---|
| Core | StorageV2, Standard_RAGRS | **Standard_RAGZRS**: zone-redundant, plus a readable geo copy in UK West that needs no infrastructure of ours |
| Public | StorageV2, Standard_LRS | **Standard_RAGZRS**, as core |
| Logging (SQL audit) | StorageV2, Standard_GZRS | Keep GZRS |
| Notifier function app storage | StorageV2, Standard_LRS | **Standard_RAGZRS**, as core. It holds the email subscriptions (`Subscriptions`, `PendingSubscriptions`, `ApiSubscriptions` tables). |
| Publisher function app storage | StorageV2, Standard_GZRS | **Standard_RAGZRS**, as core. It holds the release publishing statuses. |
| Other function app storage (Importer, Analytics, NL Search, Search Docs, Screener, Data Processor) | StorageV2, Standard_LRS | **Standard_ZRS**. Runtime state only, so a geo copy adds nothing. |
| Screener logs, search docs storage | StorageV2, Standard_LRS | **Standard_ZRS** |
| Public API data files, Analytics file share | FileStorage, Premium_ZRS | Keep Premium_ZRS. Premium file shares can't be geo-redundant, so the file share backup policy (`DailyPolicy`) uses the vault tier instead: each daily backup is copied into the geo-redundant Recovery Services vault, with 7 days of snapshots kept in the account for quick restores. Today's backups are snapshots only, which stay in the same account. |

**Dev and Test** keep zone redundancy everywhere, but core, public, Notifier and Publisher use **Standard_ZRS** rather than RAGZRS. Most of the extra cost is in the geo copy. Adding it, or removing it, is a SKU change that a normal deploy can make, so these environments can add it later. Moving between LRS and ZRS can't be done by a deploy, so zone redundancy is set from the start. Pre-Production keeps RAGZRS to match Production for DR testing. The vaulted file share backups apply in every environment, because a policy can't change tier in place and it lets restores be tested in Dev. The SKUs are set in each pipeline's `parameters/elz` files.

Also:
- **Set `allowBlobPublicAccess: false` explicitly.** No template sets it, so the Azure default applies.
- **`allowSharedKeyAccess`:** split the accounts that must keep keys (Azure Files mounts, the Functions content share, SQL auditing) from the data accounts that can go identity-only. See [section 7](#7-identity-and-network-access).

### 6.5 Databases
**Proposed.**
- **Azure SQL:** every database has `zoneRedundant: false`. Turn it on ([decision 19](#1-key-decisions), section 6.10). Entra-only authentication is covered in [section 7](#7-identity-and-network-access).
- **SQL `publicNetworkAccess`:** Prod is still Enabled (the default at [core/main.bicep:86](infrastructure/templates/core/main.bicep#L86)), while Dev, Test and Pre-Prod disable it. Build ELZ with it Disabled everywhere.
- **SQL tiers:** Content uses DTU Standard on Dev, Test and Pre-Prod but vCore serverless on Prod. DTU Standard can't be zone-redundant †, so use vCore (serverless or provisioned) wherever zone redundancy is wanted. That also makes non-Prod behave like Prod.
- **PostgreSQL:** HA is Disabled in every environment, and the Burstable tier used everywhere (B1ms, B2s in Prod) doesn't support HA †. Zone-redundant HA means moving to General Purpose, which roughly doubles the compute cost because a standby runs in a second zone. Update the API version from `2023-03-01-preview` to a GA version.

### 6.6 Monitoring
**Decided** ([decision 17](#1-key-decisions)). Move the last instrumentation-key uses to connection strings as part of the move. Ingestion by instrumentation key stopped being supported on 31 March 2025 †.
- App Services: `APPINSIGHTS_INSTRUMENTATIONKEY` and `AppInsights__InstrumentationKey` ([app-service.bicep:166-173](infrastructure/templates/common/components/app-service/app-service.bicep#L166-L173)). Admin also sends its key to the browser.
- The Data Processor: `APPINSIGHTS_INSTRUMENTATIONKEY` ([durableFunctionApp.bicep:415](infrastructure/templates/public-api/components/durableFunctionApp.bicep#L415)).
- The Container Apps environment: `daprAIInstrumentationKey` ([containerAppEnvironment.bicep:48](infrastructure/templates/public-api/components/containerAppEnvironment.bicep#L48)).

The function apps and the Public API Container App already use connection strings.

### 6.7 Other services
**Proposed.**
| Resource | Today | Suggestion |
|---|---|---|
| AI Search | `basic`, 1 replica, 1 partition | Two replicas in Prod for the read SLA, three for read-write †. Changing SKU means recreating the service, so choose the tier now. |
| SignalR (Admin) | Standard_S1 ×1; Free_F1 on Test | Keep; use identity rather than the hand-made connection string |
| Container Registry | Standard, Dev only, API `2022-02-01-preview` | Moves to the shared tooling resource group ([decision 13](#1-key-decisions)). Update to a GA API version. Premium is needed if the registry gets a private endpoint †. |
| Key Vault | standard; `enableRbacAuthorization` not set | Set it explicitly to `true` ([section 7](#7-identity-and-network-access)) |
| Container Apps environment | Workload profiles; `internal: true` | Keep. The `internal` setting and the subnet can't be changed later. |
| Static Web App (API docs) | Free; Standard on Prod | Keep. Private endpoints need Standard †. |
| Logic App (Slack alerts) | Consumption | Drop it if the Slack webhooks are removed (EES-7608) |
| Recovery Services and Backup vaults | Geo-redundant | Keep |
| Unused Basic ACR module | [public-api/components/containerRegistry.bicep](infrastructure/templates/public-api/components/containerRegistry.bicep) isn't referenced | Delete |

### 6.8 Runtime versions to watch
- **Node 22** (Public Site image `node:22.23.3-alpine`, `NodeVersion` in the build): end of life April 2027 †. Plan a move to Node 24 LTS.
- **Python 3.14** (NL Search): check that it's GA on Linux Elastic Premium before the ELZ build †.

### 6.9 API versions and leftovers
Low priority, but cheap to do while every template is being touched:
- **Preview or old API versions:** `Microsoft.ContainerRegistry/registries@2022-02-01-preview`, PostgreSQL `@2023-03-01-preview`, `Microsoft.Insights/diagnosticSettings@2021-05-01-preview` (6 files), `Microsoft.Web/serverfarms@2022-03-01`, `Microsoft.Insights/actionGroups@2019-06-01`, `Microsoft.Network/privateDnsZones@2020-06-01`. `metricAlerts@2018-03-01` and `DataFactory@2018-06-01` are still the current GA versions †.
- **The legacy `infrastructure/infrastructure-pipeline.yml`** still runs on `windows-latest` and publishes an `arm-template` artifact, but no ARM templates deploy resources any more. Retire it.

### 6.10 Redundancy
**Decided** ([decision 19](#1-key-decisions)): no failover, as the service doesn't demand it. Geo-redundant copies and backups that need no secondary deployment, and zone redundancy within UK South only where it costs little, in Pre-Prod and Prod. A zone outage is handled automatically where zone redundancy is on; elsewhere the affected service waits for the zone to recover. A full region outage means restoring from geo-redundant backups or storage copies, not failing over to a standing secondary.

Zone redundancy isn't failover. Azure spreads a resource's instances across UK South's three availability zones, so the others keep serving if one zone fails, with nothing for anyone to do. It doesn't help with a whole-region outage.

| Resource | ELZ setting | Cost and constraints |
|---|---|---|
| Storage accounts | See [section 6.4](#64-storage-redundancy-and-access) | None of note |
| Azure SQL databases | Not zone redundant; keep geo-redundant backup storage | Zone redundancy would add a compute surcharge, and needs vCore rather than DTU Standard †. It can be turned on later |
| PostgreSQL | No high availability; keep geo-redundant backups (already on in Pre-Prod and Prod) | Zone-redundant HA would need General Purpose rather than Burstable †, and its standby doubles compute |
| App Service plans | **Done.** Admin, Content API, Data API and Public Site: zone redundant in Pre-Prod and Prod (`zoneRedundantAppServicePlans` in the release pipeline's `parameters/elz` files). Importer, Notifier and Publisher: not zone redundant | Premium v2 to v4 only, and at least 2 instances †. The four web plans already run Premium with autoscaling from 2 instances, so it costs little. The Importer would need a second instance, and Notifier and Publisher (B1/S1) would need Premium. It can be switched on for an existing Premium plan by a deploy, as long as the plan's scale unit supports zones, which the zone-redundant web plans in the same resource group make sure of |
| Elastic Premium function plans (Data Processor, Analytics, Screener) | Not zone redundant | Would need at least 2 always-ready instances †. Can only be set at creation: adding it later means a new plan and app side by side. Their work is queue, timer or orchestration driven, with zone-redundant storage, so a zone outage delays it rather than losing it |
| Container Apps environment | **Done.** Zone redundant in Pre-Prod and Prod (`containerAppEnvironmentZoneRedundant` in the Public API pipeline's `parameters/elz` files) | No charge for the environment itself. Set at creation only; needs the VNet-integrated environment we already use. The Public API container app needs at least 2 replicas to benefit, and runs a minimum of 1 in Prod and 0 in Pre-Prod today |
| Application Gateway and public IP | Zones 1-3 | Already the case today |
| AI Search | 1 replica, not zone redundant | 2 replicas or more would spread across zones automatically †, but each replica is billed |
| Container Registry | Zone redundancy † | Moves to the shared tooling group |
| Key Vault, Event Grid, Front Door, Log Analytics | Built in, nothing to set † | |
| Recovery Services and Backup vaults | Geo-redundant, cross-region restore on, in every environment. Done: the Recovery Services vault already had both; the Backup vault gets cross-region restore from `backupVaultCrossRegionRestore` in the core `parameters/elz` files | Vault redundancy can't change once items are protected †. Cross-region restore costs extra, and secondary-region restore points lag by up to 36 hours |

**Decided:** zone redundancy goes in Pre-Prod and Prod (so it's rehearsed), and not in Dev or Test.

### 6.11 Content API hostnames
**Decided** ([decision 18](#1-key-decisions)). Today:

| Environment | Today | ELZ |
|---|---|---|
| Dev | `content.dev.explore-education-statistics.service.gov.uk` | `cont.dev.explore-education-statistics.service.gov.uk` |
| Test | `content.test.explore-education-statistics.service.gov.uk` | `cont.test.explore-education-statistics.service.gov.uk` |
| Pre-Prod | `cont.pre-production.explore-education-statistics.service.gov.uk` | Unchanged |
| Prod | `content.explore-education-statistics.service.gov.uk` | `cont.explore-education-statistics.service.gov.uk` |

Pre-Prod is probably `cont.` because `content.pre-production.…` is 66 characters, over the 64-character limit for a certificate's common name.

Things to handle:
- **Prod is public.** Anything calling `content.explore-education-statistics.service.gov.uk` directly (external consumers, cached links, other services) would break. Keep `content.` as a second Front Door custom domain pointing at the same origin for a transition period, and decide when to retire it.
- **Update every reference.** The `contentApiUrl` / `contentApi` params in core, Public API and search, the Robot test `.env` files, and frontend config.
- **DNS and certificates** for the new names, done alongside the Front Door custom domain validation in the new subscription.

### 6.12 Data processor Durable Functions storage
**Decided** ([decision 20](#1-key-decisions)): keep the Azure Storage backend, connected by managed identity.

Today the processor has three storage accounts, all connected by Key Vault-stored connection strings ([durableFunctionApp.bicep:157-159,391-440](infrastructure/templates/public-api/components/durableFunctionApp.bicep#L157-L159)):

| Account | Used for | ELZ connection |
|---|---|---|
| `…mg` | Function host storage (`AzureWebJobsStorage`) | Identity: `AzureWebJobsStorage__accountName` plus the processor's user-assigned identity (`__credential: managedidentity`, `__clientId`) † |
| `…s1`, `…s2` | Each slot's orchestration state (`DurableManagementStorage`) | Identity: `DurableManagementStorage__accountName` plus the same identity settings † |
| `…s1`, `…s2` | Each slot's code file share (`WEBSITE_CONTENTAZUREFILECONNECTIONSTRING`) | Still key-based. Elastic Premium content shares don't support identity †, so these two accounts keep shared key access |

Why decide now: orchestrations can't be moved between backends or task hubs, so a later change means draining the processor. The new environment starts empty.

What's needed:
- **Role assignments** for the processor's identity on each account: Storage Blob Data Owner, Storage Queue Data Contributor and Storage Table Data Contributor †.
- **Keep the per-slot split** so staging and production don't pick up each other's orchestrations during a swap.
- **Remove the connection-string secrets** that are no longer used. Only the content share connection strings remain.
- **Use Durable Functions extension settings that support identity.** Check the version in the processor project †.

The managed Durable Task Scheduler backend was considered and not chosen, as the bigger change for this move.

## 7. Identity and network access

These are hard to change once the environments exist, so they belong in the initial build.

- **User-assigned managed identity per app**, created first. The names end up in role assignments and SQL contained users.
- **SQL: Entra-only authentication** (`azureADOnlyAuthentication`). Today the apps use SQL logins with passwords. PostgreSQL already uses Entra only. Keep the collation `SQL_Latin1_General_CP1_CI_AS`.
- **Split storage accounts into key-based and identity-only.** Azure Files mounts, the Functions content share and SQL auditing still need account keys. The data accounts (core, public, importer, publisher, notifier) could then set `allowSharedKeyAccess: false`, after code changes to the storage clients (including EES-4202).
- **Move other secrets to identity:** Durable Functions storage, ACR pulls (service principal password today), SignalR, Azure OpenAI and App Insights (instrumentation keys).
- **Key Vault:** set `enableRbacAuthorization: true` explicitly.
- **Pipeline service connections** using workload identity federation.
- **Private endpoints with public access off**, replacing the service endpoints and VNet rules on SQL and storage. Private DNS zones would be needed for `vault`, `signalr`, `search`, `azurecr` and Azure Monitor, unless the ELZ provides central zones.
- **No fixed outbound IPs needed.** Nothing external allowlists our apps' outbound IPs. The EES-5446 TODOs are about the *pipeline agents'* IPs reaching our resources (the `AzureCloud` service-tag rule and the open SCM site in the Public API templates). They go away once the agents reach us over the private network.
- **Self-hosted pipeline runners with network access to each environment.** Once public access is off, deploys and SQL migrations need a network path in. The agent VMSS will sit in the shared tooling resource group with the Container Registry ([decision 13](#1-key-decisions)), connected to the environment VNets.

## 8. Open questions

For the DfE platform team. Their answers could change [section 5](#5-subnet-migration) and [section 7](#7-identity-and-network-access).

1. Can we have a /21 per environment? The ranges allocated (Dev `10.226.192.0/24`, Test `10.221.192.0/24`, Pre-Prod `10.216.192.0/25`, Prod `10.216.192.128/25`) can't hold our subnets; see [section 5](#5-subnet-migration).
2. Is there a hub VNet, and does traffic between subnets go through a firewall (route tables)?
3. Does policy require one subnet per trust zone, or NSGs on every subnet?
4. Are private DNS zones central (managed by policy), or ours to create?
5. How does outbound internet traffic leave our spoke (e.g. forced through the hub firewall)? If so, what do we need to request so our apps can reach GOV.UK Notify and other external APIs? We don't need fixed outbound IPs.
6. How should the shared tooling resource group (Container Registry and agent VMSS) connect to the environment VNets: VNet peering to each, or through the hub? Can it reach Test, Pre-Prod and Prod as well as Dev?

## 9. Follow-up work

Shortcuts taken along the way that need revisiting.

| # | Item | Where | Why it was deferred |
|---|---|---|---|
| 1 | Make the Robot UI tests job use `get-resource-names.yml` for its Key Vault name, instead of building the name in its own Bash step. Remove the hand-built legacy and ELZ names, and the `TODO EES-7502`, when doing so. | `infrastructure/templates/main-app-release/ci/jobs/run-robot-tests.yml` | `get-resource-names.yml` needs the Azure CLI, which isn't installed on the self-hosted `ees-ubuntu2604-large-v2` agents the job runs on. Needs the CLI added to `infrastructure/scripts/vmss-agent-bootstrap.sh` and the scale sets reimaged first. |
| 2 | Delete the secrets that are no longer written from the existing legacy Key Vaults: the `*-access-key` secrets; the Public API data, Analytics and screener logs storage connection strings; `ees-admin-connectionstring-publicdatadb`, `ees-publisher-connectionstring-publicdatadb`, `ees-publicapi-data-processor-connectionstring-publicdatadb` and `ees-publicapi-public-api-containerapp-private-url`. | Each environment's `s101…-kv-ees-01` | Bicep doesn't delete secrets it stops managing. They'd be left holding storage keys that are no longer refreshed. Check first that nobody uses them by hand. |
