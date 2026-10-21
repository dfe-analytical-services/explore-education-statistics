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
| 1 | Naming | Pattern `{sub}{type}-uks-{workload}{nn}`, e.g. `s292d01app-uks-dataapi01`. | Decided |
| 2 | Naming | Storage accounts drop the `01` and all hyphens, and stay within 24 characters, e.g. `s292d01stukscore`. | Decided |
| 3 | Naming | Workload names `dataapi` and `contentapi` (not `data` and `content`); shared resources use `ees`. | Decided |
| 4 | Naming | The `papi` sub-project is combined, unhyphenated, into the workload (`papiprocessor`). `sapi` is dropped (just `screener`). | Decided |
| 5 | Naming | Standard CAF/azd abbreviations (`app`, `func`, `plan`, `appi`, `st`, `sigr`, `logic`, `adf`, ...) for ELZ; the legacy set stays for existing environments. | Decided |
| 6 | Naming | Generic Front Door public site names (`-fdog`, `-fdo`, `-fdr`) get purpose names (`publicsite`). | Decided |
| 7 | Naming | Child resources of the App Gateway use short `{workload}-{purpose}` names without the subscription prefix, e.g. `publicsite-backend`. Request routing rules and path rules end in `-rule`, e.g. `papi-routing-rule`, `papidocs-path-rule`. | Decided |
| 8 | Naming | Key Vault certificates and secrets drop the `ees-` prefix and any subscription prefixes. The Key Vault is per-environment and EES-only, so the prefixes add nothing. They're named `{workload}-{purpose}`, using the same workload names as the resources (`publicsite`, `contentapi`, `dataapi`, `papi`, `papiprocessor`, ...), with `connection-string` always hyphenated. For example `publicsite-certificate`, `publicsite-basic-auth-username`, `admin-sql-password`, `publisher-publicdatadb-connection-string`. | Decided |
| 9 | Naming | Storage connection-string secrets are named `{workload}-storage-connection-string`, e.g. `core-storage-connection-string`, `importer-storage-connection-string`. | Decided |
| 10 | Mechanism | One `elz` flag selects ELZ names; per-environment ELZ values go in `parameters/elz/` param files, picked by one pipeline variable. | Decided |
| 11 | Mechanism | Every pipeline reads names from `infrastructure/templates/common/resource-names.bicep` (Bicep) or `get-resource-names.yml` (YAML). | Done (stack #7429) |
| 12 | Subscriptions | Remove all subscription-ID allow-lists (e.g. `frontDoor.bicep`'s `@allowed` list). s180 is no longer a target. | Decided |
| 13 | Resource groups | The Container Registry and the pipeline agent VM scale sets move into their own resource group, separate from the Dev resource group but connected to it. Its name and network connection (e.g. VNet peering) are still to be decided. | Decided |
| 14 | Hosting | Linux for every App Service and Function App. | Decided |
| 15 | Network | Private endpoints instead of service endpoints, with public network access off from the start. | Decided |
| 16 | Identity | Managed identity instead of connection strings and keys wherever the service supports it. | Decided |
| 17 | Monitoring | Every app sends telemetry to Application Insights by connection string; the remaining instrumentation-key uses are removed as part of the move. See [section 6.6](#66-monitoring). | Decided |
| 18 | Hostnames | The Content API uses `cont.` in every environment: `cont.dev.`, `cont.test.`, `cont.pre-production.` and `cont.` for Prod, all on `explore-education-statistics.service.gov.uk`. Only Pre-Prod uses it today. See [section 6.11](#611-content-api-hostnames). | Decided |
| 19 | Redundancy | The safest redundancy available without standing up failover infrastructure: zone redundancy within UK South wherever the service supports it, plus geo-redundant copies and backups that need no secondary deployment. No active secondary region. See [section 6.10](#610-redundancy). | Decided |
| 20 | Durable Functions | The data processor keeps the Azure Storage backend for orchestration state, but connects to it with its managed identity instead of connection strings. See [section 6.12](#612-data-processor-durable-functions-storage). | Decided |
| 21 | Network | Subnet layout: one integration subnet per App Service plan, two shared private endpoint subnets, dedicated App Gateway and Container Apps subnets. See [section 5](#5-subnet-migration). | Proposed / Blocked |
| 22 | Network | VNet and subnet address ranges come from the `.bicepparam` files (EES-7425). | Done |
| 23 | Network | Actual ELZ IP ranges. | Decided (ranges received) |

## 2. Actions

Status: **Done** (merged), **In progress** (in review or uncommitted), **To do**, **Blocked**.

| # | Action | Status | Notes |
|---|---|---|---|
| 1 | Move the VNet and subnet address ranges into the core `.bicepparam` files | Done | EES-7425 |
| 2 | Build the shared naming module `common/resource-names.bicep`, reproducing today's names exactly | In progress | Stack #7429; first PRs merged |
| 3 | Move every pipeline (Bicep and YAML) onto the shared names, verified with identical snapshots | In progress | Stack #7429. YAML pipelines use `get-resource-names.yml` |
| 4 | Add the `elz` flag to `ResourceNamingContext`, the ELZ abbreviations (`abbreviations-elz.bicep`) and `getElzResourceNames()` | In progress | Legacy output verified unchanged. ELZ output matches [section 4](#4-resource-naming) except the interim subnets |
| 5 | Make the Robot UI tests job pick its Key Vault name from the `elz` flag | In progress | Interim Bash step; see [follow-up 1](#9-follow-up-work) |
| 6 | Pass the `elz` flag into every pipeline's `main.bicep`, and create the `parameters/elz/` param files | To do | The flag comes from an `elz` variable in each environment's variable group |
| 7 | Let `get-resource-names.yml` take the `elz` flag | To do | Set `RESOURCE_NAMES_ELZ=true` only when `$(elz)` is exactly `true` |
| 8 | Add `elz: true` and `subscription` to the s292 environments' variable groups | To do | Once the environments exist |
| 9 | Send the questions in [section 8](#8-open-questions) to the platform team | To do | Draft message written |
| 10 | Get the ELZ IP ranges | Done | Received from the platform team |
| 10a | Put the ELZ IP ranges into the `parameters/elz/` param files, re-planning the subnet blocks per [section 5](#5-subnet-migration) | To do | |
| 11 | Apply naming decisions 7-9 (App Gateway child names, Key Vault certificate and secret names, storage connection-string secret names) in `getElzResourceNames()` | In progress | Core, public, Importer, Notifier, Publisher and Screener storage modules now take their connection-string secret name from `resourceNames`. Legacy output verified unchanged |
| 11a | Move the Key Vault secret names that are still hardcoded into `resourceNames`, so they follow the `elz` flag | To do | About 15 in Bicep (e.g. `ees-alerts-slackconfig`, `ees-sql-admin-password`, `ees-publicapi-app-gateway-fuapi-header`) and the Robot tests and Public API pipeline YAML |
| 12 | Confirm the subnet layout, then change the `ResourceNames` subnets and the VNet module to match | Blocked | Needs answers to open questions 2 and 3. See [section 5](#5-subnet-migration) |
| 13 | Move names still built inside modules into `resourceNames`: private endpoints, alerts, the private DNS VNet link, the resource group | To do | They won't follow the `elz` flag until then |
| 14 | Remove the subscription allow-lists, e.g. the `@allowed` list in `frontDoor.bicep` | To do | Decision 12 |
| 15 | Create the shared tooling resource group for the Container Registry and agent VMSS, and agree its name and network connection | To do | Decision 13; open question 6 |
| 16 | Add the Azure CLI to `vmss-agent-bootstrap.sh` and reimage the agent scale sets | To do | Unblocks follow-up 1 |
| 17 | Move the Windows apps to Linux: publish flags, `linuxFxVersion`, app settings, testing | To do | [Section 6.2](#62-windows--linux) |
| 18 | Replace the remaining App Insights instrumentation keys with connection strings | To do | [Section 6.6](#66-monitoring) |
| 19 | Connect the data processor's Durable Functions storage with managed identity | To do | [Section 6.12](#612-data-processor-durable-functions-storage) |
| 20 | Apply the redundancy settings, and decide which environments get them | To do | [Section 6.10](#610-redundancy) |
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
| `applicationGateway.globalWafPolicy` | App Gateway WAF Policy | `s101d01-ees-agw-01-global-afwp` | `s292d01waf-uks-agwglobal01` | `afwp` is the Azure Firewall abbreviation |
| `applicationGateway.sites.publicSite.backend` | AGW backend pool (child) | `s101d01-as-ees-public-site-backend` | `publicsite-backend` | NEW. Child of the gateway |
| `applicationGateway.sites.publicSite.route` | AGW routing rule (child) | `s101d01-as-ees-public-site-route` | `publicsite-routing-rule` | Decision 7: App Gateway rules end in `-rule` |
| `applicationGateway.sites.publicSite.site` | AGW listener/site (child) | `s101d01-as-ees-public-site-site` | `publicsite-site` | NEW. Child of the gateway |
| `applicationGateway.sites.publicApi.backend` | AGW backend pool (child) | `s101d01-ees-papi-ca-api` | `papi-backend` | Child of the gateway; today it reuses the container app's name |
| `applicationGateway.sites.publicApi.route` | AGW routing rule (child) | `s101d01-ees-papi` | `papi-routing-rule` | Decision 7: App Gateway rules end in `-rule` |
| `applicationGateway.sites.publicApi.site` | AGW listener/site (child) | `s101d01-ees-papi` | `papi-site` | NEW. Child of the gateway |
| `applicationGateway.sites.publicApi.wafPolicy` | App Gateway WAF Policy | `s101d01-ees-agw-01-public-api-afwp` | `s292d01waf-uks-agwpapi01` | `afwp` is the Azure Firewall abbreviation |
| `applicationGateway.sites.publicApi.rewriteSet` | AGW rewrite rule set (child) | `s101d01-ees-papi-docs-rewrites` | `papidocs-rewrites` | Decision 7: named after the docs site it rewrites for |
| `applicationGateway.sites.publicApiDocs.backend` | AGW backend pool (child) | `s101d01-ees-papi-stapp-docs` | `papidocs-backend` | Child of the gateway; today it reuses the static web app's name |
| `applicationGateway.sites.publicApiDocs.pathRules.backend` | AGW path rule (child) | `docs-backend` | `papidocs-path-rule` | Decision 7: workload-first, like the other App Gateway child names |
| `applicationGateway.sites.publicApiDocs.pathRules.rootRedirect` | AGW path rule (child) | `docs-root-redirect` | `papidocs-root-redirect-rule` | Decision 7: App Gateway rules end in `-rule` |
| `backups.backupVault` | Backup Vault | `s101d01-ees-bvault` | `s292d01bvault-uks-ees01` |  |
| `backups.backupVaultBlobsPolicy` | Backup Vault Policy | `s101d01-ees-blob-bkpol` | `s292d01bkpol-uks-blob01` |  |
| `backups.backupVaultPsqlFlexibleServerPolicy` | Backup Vault Policy | `s101d01-ees-psql-flexible-server-bkpol` | `s292d01bkpol-uks-psql01` | Third PostgreSQL spelling removed |
| `backups.recoveryServicesVault` | Recovery Services Vault | `s101d01-ees-rsv` | `s292d01rsv-uks-ees01` |  |
| `containerAppEnvironment` | Container App Environment | `s101d01-ees-cae-01` | `s292d01cae-uks-ees01` |  |
| `dataFactory` | Data Factory | `s101d01-df-ees-release` | `s292d01adf-uks-release01` | `df`→`adf` |
| `frontDoor.frontDoorName` | Front Door Profile | `s101d01-ees-afd` | `s292d01afd-uks-ees01` | ⚠ global resource, so `uks` is misleading |
| `frontDoor.defaultEndpoint.endpointName` | Front Door Endpoint | `s101d01-ees-fde` | `s292d01fde-uks-ees01` | Endpoint names are globally unique hostnames |
| `frontDoor.wafPolicy` | Front Door WAF Policy | `s101d01eesafdwafp` | `s292d01fdfpuksees01` | Alphanumeric only; `wafp`→`fdfp` |
| `frontDoor.wafSecurityPolicy` | Front Door Security Policy | `s101d01eesafdwafsp` | `s292d01afdsp-uks-ees01` | Hyphens are allowed here |
| `frontDoor.publicSite.customDomain` | Front Door Custom Domain | `s101d01-ees-public-site-afd-domain` | `s292d01afdd-uks-publicsite01` |  |
| `frontDoor.publicSite.originGroup` | Front Door Origin Group | `s101d01-ees-fdog` | `s292d01fdog-uks-publicsite01` | Says which site (TODO EES-7502) |
| `frontDoor.publicSite.origin` | Front Door Origin | `s101d01-ees-fdo` | `s292d01fdo-uks-publicsite01` | Says which site (TODO EES-7502) |
| `frontDoor.publicSite.route` | Front Door Route | `s101d01-ees-fdr` | `s292d01fdr-uks-publicsite01` | Says which site (TODO EES-7502) |
| `frontDoor.contentApi.customDomain` | Front Door Custom Domain | `s101d01-ees-content-afd-domain` | `s292d01afdd-uks-contentapi01` |  |
| `frontDoor.contentApi.originGroup` | Front Door Origin Group | `s101d01-ees-content-fdog` | `s292d01fdog-uks-contentapi01` |  |
| `frontDoor.contentApi.origin` | Front Door Origin | `s101d01-ees-content-fdo` | `s292d01fdo-uks-contentapi01` |  |
| `frontDoor.contentApi.route` | Front Door Route | `s101d01-ees-content-fdr` | `s292d01fdr-uks-contentapi01` |  |
| `frontDoor.contentApi.allFilesCacheRoute` | Front Door Route | `s101d01-ees-content-all-files-cache-fdr` | `s292d01fdr-uks-contentapiallfilescache01` |  |
| `storageAccounts.core` | Storage Account | `s101d01saeescore` (Test/Pre-Prod `s101t01storageeescore`) | `s292d01stukscore` | Test/Pre-Prod `storage` special case removed |
| `storageAccounts.public` | Storage Account | `s101d01saeespublic` | `s292d01stukspublic` |  |
| `storageAccounts.logging` | Storage Account | `s101d01saeeslogging` | `s292d01stukslogging` |  |
| `keyVault.keyVault` | Key Vault | `s101d01-kv-ees-01` | `s292d01kv-uks-ees01` |  |
| `keyVault.keys.dataProtection` | Key Vault key | `ees-dataprotection-key` | `ees-dataprotection-key` | NEW. Unchanged: the vault is per-environment |
| `keyVault.certificates.publicSite` | Key Vault certificate | `s101d01-as-ees-public-site-certificate` | `publicsite-certificate` | Decision 8: no `ees-` or subscription prefix |
| `keyVault.certificates.contentApiFrontDoor` | Key Vault certificate | `s101d01-as-ees-content-afd-certificate` | `contentapi-afd-certificate` | Decision 8: no `ees-` or subscription prefix |
| `keyVault.certificates.publicApiAppGateway` | Key Vault certificate | `s101d01-ees-papi-certificate` | `papi-agw-certificate` | Decision 8: no `ees-` or subscription prefix |
| `alertsGroup` | Action Group | `s101d01-ag-ees-alertedusers` | `s292d01ag-uks-alertedusers01` |  |
| `alertsLogicApp` | Logic App | `s101d01-la-ees-slackwebhook` | `s292d01logic-uks-slackwebhook01` | `la`→`logic` (or drop it with the Slack webhooks, EES-7608) |
| `databases.coreSqlServer` | SQL Server | `s101d01-sqlsvr-ees-01` | `s292d01sql-uks-core01` | Named by purpose |
| `databases.publicSqlServer` | SQL Server | `s101d01-sqlsvr-ees-02` | `s292d01sql-uks-public01` | Named by purpose |
| `databases.contentDb` | SQL Database | `content` | `content` | NEW. Unchanged |
| `databases.statisticsDb` | SQL Database | `statistics` | `statistics` | NEW. Unchanged |
| `databases.postgreSqlFlexibleServer` | PostgreSQL Flexible Server | `s101d01-ees-psql-flexibleserver` | `s292d01psql-uks-ees01` | `-flexibleserver` suffix removed (TODO EES-7502) |
| `eventGrid.topics.publicationChanged` | Event Grid Topic | `s101d01-ees-evgt-publication-changed` | `s292d01evgt-uks-publicationchanged01` |  |
| `eventGrid.topics.releaseChanged` | Event Grid Topic | `s101d01-ees-evgt-release-changed` | `s292d01evgt-uks-releasechanged01` |  |
| `eventGrid.topics.releaseVersionChanged` | Event Grid Topic | `s101d01-ees-evgt-release-version-changed` | `s292d01evgt-uks-releaseversionchanged01` |  |
| `eventGrid.topics.themeChanged` | Event Grid Topic | `s101d01-ees-evgt-theme-changed` | `s292d01evgt-uks-themechanged01` |  |
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
| `publicApi.storage.fileShare` | File Share | `s101d01-ees-papi-share-data` | `s292d01share-uks-papidata01` |  |

### Screener (`sapi` dropped)

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `screener.functionApp` | Function App | `s101d01-ees-sapi-fa-screener` | `s292d01func-uks-screener01` | `sapi` dropped |
| `screener.appServicePlan` | App Service Plan | `s101d01-ees-sapi-fa-screener` | `s292d01plan-uks-screener01` | No longer reuses the function app's name (TODO EES-7502) |
| `screener.appInsights` | Application Insights | `s101d01-ees-sapi-ai-screener` | `s292d01appi-uks-screener01` | Last table had the current name wrong: it does include `sapi` |
| `screener.functionStorageAccount` | Storage Account | `s101d01eessapisafn` | `s292d01stuksscreenerfn` |  |
| `screener.logsStorageAccount` | Storage Account | `s101d01eessapisalogs` | `s292d01stuksscreenerlogs` | Exactly 24 chars |
| `screener.logsFileShare` | File Share | `s101d01-ees-sapi-fa-screener-logs-share` | `s292d01share-uks-screenerlogs01` |  |

### Search

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `nlSearch.functionApp` | Function App | `s101d01-ees-fa-nlsearch` | `s292d01func-uks-nlsearch01` |  |
| `nlSearch.appServicePlan` | App Service Plan | `s101d01-ees-asp-nlsearch` | `s292d01plan-uks-nlsearch01` |  |
| `nlSearch.functionStorageAccount` | Storage Account | `s101d01eessanlsearchfn` | `s292d01stuksnlsearchfn` |  |
| `search.service` | Search Service | `s101d01-ees-srch` | `s292d01srch-uks-ees01` |  |
| `search.storageAccount` | Storage Account | `s101d01eessasearch` | `s292d01stukssearch` |  |
| `search.appInsights` | Application Insights | `s101d01-ees-ai-search` | `s292d01appi-uks-search01` | Shared by nlSearch and searchDocs |
| `search.searchDocsEventSubscriptions.publicationArchived` | Event Grid Subscription | `s101d01-ees-evgs-publication-archived` | `s292d01evgs-uks-publicationarchived01` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.publicationChanged` | Event Grid Subscription | `s101d01-ees-evgs-publication-changed` | `s292d01evgs-uks-publicationchanged01` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.publicationDeleted` | Event Grid Subscription | `s101d01-ees-evgs-publication-deleted` | `s292d01evgs-uks-publicationdeleted01` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.publicationLatestPublishedReleaseReordered` | Event Grid Subscription | `s101d01-ees-evgs-publication-latest-published-release-reordered` | `s292d01evgs-uks-publicationlatestpublishedreleasereordered01` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.publicationRestored` | Event Grid Subscription | `s101d01-ees-evgs-publication-restored` | `s292d01evgs-uks-publicationrestored01` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.releaseSlugChanged` | Event Grid Subscription | `s101d01-ees-evgs-release-slug-changed` | `s292d01evgs-uks-releaseslugchanged01` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.releaseVersionPublished` | Event Grid Subscription | `s101d01-ees-evgs-release-version-published` | `s292d01evgs-uks-releaseversionpublished01` | NEW (last table only had a `{topic}` placeholder) |
| `search.searchDocsEventSubscriptions.themeChanged` | Event Grid Subscription | `s101d01-ees-evgs-theme-changed` | `s292d01evgs-uks-themechanged01` | NEW (last table only had a `{topic}` placeholder) |
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
| `analytics.storage.fileShareName` | File Share | `s101d01-ees-share-anlyt` | `s292d01share-uks-analytics01` | Child of the storage account, so the prefix is optional |

### Key Vault secrets named after a storage account today

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `keyVault.secrets.admin.screenerStorageAccountConnectionString` | Key Vault secret | `s101d01eessapisafn-connection-string` | `screener-storage-connection-string` | Decision 9 |
| `keyVault.secrets.importerStorageAccountConnectionString` | Key Vault secret | `s101d01eessaimporter-connection-string` | `importer-storage-connection-string` | Decision 9 |
| `keyVault.secrets.notifierStorageAccountConnectionString` | Key Vault secret | `s101d01saeesnotify-connection-string` (Test/Pre-Prod `s101t01storageeesnotify-connection-string`) | `notifier-storage-connection-string` | Decision 9 |
| `keyVault.secrets.publisherStorageAccountConnectionString` | Key Vault secret | `s101d01saeespublisher-connection-string` | `publisher-storage-connection-string` | Decision 9 |

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
| `keyVault.secrets.publisher.publicDataDbConnectionString` | `ees-publisher-connectionstring-publicdatadb` | `publisher-publicdatadb-connection-string` |
| `keyVault.secrets.screener.appRegistrationClientId` | `ees-screener-app-registration-client-id` | `screener-app-registration-client-id` |
| `keyVault.secrets.publicApiContainerAppPrivateUrl` | `ees-publicapi-public-api-containerapp-private-url` | `papi-private-url` |
| `keyVault.secrets.bauEmail` | `ees-bau-email` | `bau-email` |
| `keyVault.secrets.coreStorageAccountConnectionString` | `ees-storage-core` | `core-storage-connection-string` |
| `keyVault.secrets.publicStorageAccountConnectionString` | `ees-storage-public` | `public-storage-connection-string` |

## 5. Subnet migration

Recommended on 15 Oct 2026. **Not yet confirmed** with the DfE platform team; see [Open questions](#8-open-questions).

- **One integration subnet per App Service plan**, delegated and holding nothing else. About /26 each, inside a /24 block reserved per workload so a second app can be added later. Named by what the app does, not its resource type.
- **Two shared private endpoint subnets** replace the 10 per-workload `-pep` subnets:
  - `s292d01snet-uks-datapep01`: SQL, PostgreSQL, storage, Key Vault, Event Grid, Search, SignalR, ACR.
  - `s292d01snet-uks-appspep01`: inbound private endpoints to the apps themselves.
  - Set `privateEndpointNetworkPolicies` on both, put each private endpoint in an Application Security Group, and write NSG rules from each app's integration subnet to those groups.
- **Dedicated subnets** kept for the App Gateway and the Container Apps environment.

**Open choice: one or two private endpoint subnets.** Two subnets separate the data tier from inbound app endpoints. That allows a different route table (e.g. sending app-to-app traffic through a hub firewall), a blanket NSG rule per tier, and fits a subnet-per-trust-zone policy. With network policies and an Application Security Group per endpoint, a single subnet gives the same per-resource control and is simpler, and the apps subnet would only hold about four endpoints. Decide using the answers to [open questions](#8-open-questions) 2 and 3: if either is yes, keep two. Endpoints have to be recreated to move subnet, so settle this before the build.

23 subnets today become 16:

| Group | Subnets |
|---|---|
| Integration (12) | `admin01`, `contentapi01`, `dataapi01`, `publicsite01` (new), `importer01`, `notifier01`, `publisher01`, `screener01`, `papiprocessor01`, `nlsearch01`, `searchdocs01`, `analytics01`. A second app in a workload (e.g. a screener UI) gets its own subnet from the same /24 block |
| Dedicated (2) | `agw01`, `cae01` |
| Shared private endpoints (2) | `datapep01`, `appspep01` |

All prefixed `s292d01snet-uks-`. New subnets with no current equivalent, to add to `resourceNames.vnet.subnets`:

| Proposed `resourceNames` path | Type | Proposed (s292 Dev) | Notes |
|---|---|---|---|
| `vnet.subnets.publicSite` | Subnet (integration) | `s292d01snet-uks-publicsite01` | The public site has no VNet integration today; it needs it once the Content API is private |
| `vnet.subnets.dataPrivateEndpoints` | Subnet (private endpoints) | `s292d01snet-uks-datapep01` | Replaces 6 subnets outright and takes the storage side of 4 more |
| `vnet.subnets.appsPrivateEndpoints` | Subnet (private endpoints) | `s292d01snet-uks-appspep01` | Takes the function apps' inbound PEs from 4 subnets |

### Existing subnets and where they go

Until this layout is confirmed, `getElzResourceNames()` in `resource-names.bicep` keeps today's one-to-one set of subnets under ELZ names, so the VNet templates work unchanged. The private endpoint subnets are named `s292d01snet-uks-sqlpep01`, `…-psql01`, `…-evgtpep01`, `…-papistpep01`, `…-searchstpep01`, `…-analyticsstpep01`, `…-screenerpep01`, `…-papiprocessorpep01`, `…-nlsearchpep01` and `…-searchdocspep01`. Moving to the shared subnets below also means changing the `ResourceNames` type and the VNet module.

| `resourceNames` path | Type | Current (Dev) | Proposed (s292 Dev) | Notes |
|---|---|---|---|---|
| `vnet.subnets.applicationGateway` | Subnet (dedicated) | `s101d01-ees-snet-agw-01` | `s292d01snet-uks-agw01` | Kept: App Gateway needs its own subnet |
| `vnet.subnets.containerAppEnvironment` | Subnet (dedicated, `Microsoft.App/environments`) | `s101d01-ees-snet-cae-01` | `s292d01snet-uks-cae01` | Kept: the CAE needs its own subnet |
| `vnet.subnets.admin` | Subnet (integration) | `s101d01-snet-ees-admin` | `s292d01snet-uks-admin01` |  |
| `vnet.subnets.contentApi` | Subnet (integration) | `s101d01-snet-ees-content` | `s292d01snet-uks-contentapi01` |  |
| `vnet.subnets.dataApi` | Subnet (integration) | `s101d01-snet-ees-data` | `s292d01snet-uks-dataapi01` |  |
| `vnet.subnets.importer` | Subnet (integration) | `s101d01-snet-ees-importer` | `s292d01snet-uks-importer01` |  |
| `vnet.subnets.notifier` | Subnet (integration) | `s101d01-snet-ees-notify` | `s292d01snet-uks-notifier01` | `notify`→`notifier` |
| `vnet.subnets.publisher` | Subnet (integration) | `s101d01-snet-ees-publisher` | `s292d01snet-uks-publisher01` |  |
| `vnet.subnets.screenerFunctionApp` | Subnet (integration) | `s101d01-ees-sapi-snet-fa-screener` | `s292d01snet-uks-screener01` | `sapi` and `fa` dropped |
| `vnet.subnets.publicApiDataProcessor` | Subnet (integration) | `s101d01-ees-papi-snet-fa-processor` | `s292d01snet-uks-papiprocessor01` | `fa` dropped |
| `vnet.subnets.nlSearchFunctionApp` | Subnet (integration) | `s101d01-ees-snet-fa-nlsearch` | `s292d01snet-uks-nlsearch01` | `fa` dropped |
| `vnet.subnets.searchDocsFunctionApp` | Subnet (integration) | `s101d01-ees-snet-fa-searchdocs` | `s292d01snet-uks-searchdocs01` | `fa` dropped |
| `vnet.subnets.analyticsFunctionApp` | Subnet (integration) | `s101d01-ees-snet-fa-analytics` | `s292d01snet-uks-analytics01` | `fa` dropped |
| `vnet.subnets.sqlServerPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-sqlsvr-pep` | `s292d01snet-uks-datapep01` | REMOVED: merged into the shared data PE subnet (both SQL servers) |
| `vnet.subnets.psqlFlexibleServer` | Subnet (private endpoints) | `s101d01-ees-snet-psql-flexibleserver` | `s292d01snet-uks-datapep01` | REMOVED: merged into the shared data PE subnet. Not delegated today: it only holds the PostgreSQL private endpoint |
| `vnet.subnets.eventGridCustomTopicPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-evgt-pep` | `s292d01snet-uks-datapep01` | REMOVED: merged into the shared data PE subnet (Event Grid topics) |
| `vnet.subnets.publicApiStoragePrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-papi-snet-sa-pep` | `s292d01snet-uks-datapep01` | REMOVED: merged into the shared data PE subnet (Public API storage) |
| `vnet.subnets.searchStoragePrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-sa-search-pep` | `s292d01snet-uks-datapep01` | REMOVED: merged into the shared data PE subnet (search storage, Search service) |
| `vnet.subnets.analyticsStoragePrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-sa-anlyt-pep` | `s292d01snet-uks-datapep01` | REMOVED: merged into the shared data PE subnet (analytics storage) |
| `vnet.subnets.screenerStoragePrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-sapi-snet-sa-screener-pep` | `s292d01snet-uks-datapep01 + s292d01snet-uks-appspep01` | REMOVED: split. Storage PEs go to the data subnet; the screener function app's inbound PE goes to the apps subnet |
| `vnet.subnets.publicApiDataProcessorPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-papi-snet-fa-processor-pep` | `s292d01snet-uks-datapep01 + s292d01snet-uks-appspep01` | REMOVED: split. Durable Functions storage PEs go to the data subnet; the processor's inbound PE goes to the apps subnet |
| `vnet.subnets.nlSearchFunctionAppPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-fa-nlsearch-pep` | `s292d01snet-uks-datapep01 + s292d01snet-uks-appspep01` | REMOVED: split. Function storage PEs go to the data subnet; the function app's inbound PE goes to the apps subnet |
| `vnet.subnets.searchDocsFunctionAppPrivateEndpoints` | Subnet (private endpoints) | `s101d01-ees-snet-fa-searchdocs-pep` | `s292d01snet-uks-datapep01 + s292d01snet-uks-appspep01` | REMOVED: split, as above |

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
| All function app storage (Importer, Notifier, Publisher, Analytics, NL Search, Search Docs, Screener, Data Processor) | StorageV2, Standard_LRS (Publisher GZRS) | **Standard_ZRS**. Runtime state only, so a geo copy adds nothing. |
| Screener logs, search docs storage | StorageV2, Standard_LRS | **Standard_ZRS** |
| Public API data files, Analytics file share | FileStorage, Premium_ZRS | Keep Premium_ZRS. Premium file shares can't be geo-redundant. |

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
**Decided** ([decision 19](#1-key-decisions)): zone redundancy within UK South wherever it's supported, and geo-redundant copies and backups that need no secondary deployment. A zone outage is handled automatically. A full region outage means restoring from geo-redundant backups or storage copies, not failing over to a standing secondary.

| Resource | ELZ setting | Cost and constraints |
|---|---|---|
| Storage accounts | See [section 6.4](#64-storage-redundancy-and-access) | None of note |
| Azure SQL databases | `zoneRedundant: true`; keep geo-redundant backup storage | Needs vCore, not DTU Standard † |
| PostgreSQL | Zone-redundant HA; keep geo-redundant backups (already on in Pre-Prod and Prod) | Needs General Purpose, not Burstable †; standby doubles compute |
| App Service plans | Zone redundant (`zoneRedundant: true`) | Premium tiers only, and at least 2 instances †. Notifier and Publisher (B1/S1 today) would need Premium. Set at creation |
| Elastic Premium function plans | Zone redundant | Minimum instance count applies †; set at creation |
| Container Apps environment | `zoneRedundant: true` | Set at creation only; needs the VNet-integrated environment we already use |
| Application Gateway and public IP | Zones 1-3 | Already the case today |
| AI Search | 2 replicas or more, which spreads across zones automatically † | Each replica is billed |
| Container Registry | Zone redundancy † | Moves to the shared tooling group |
| Key Vault, Event Grid, Front Door, Log Analytics | Built in, nothing to set † | |
| Recovery Services and Backup vaults | Geo-redundant, cross-region restore on | Vault redundancy can't change once items are protected † |

**Open (Proposed):** which environments get this. Zone redundancy multiplies instance counts, and PostgreSQL HA needs a bigger tier. The suggestion is Prod and Pre-Prod (so it's rehearsed), with Dev and Test on the cheaper non-redundant settings. The creation-only settings (App Service plans, Container Apps environment, vaults) are then set correctly from day one where they matter.

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

1. ~~What IP range do we get?~~ Answered: ranges received.
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
