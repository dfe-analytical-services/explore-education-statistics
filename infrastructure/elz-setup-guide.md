# Setting up an environment in the DfE Enterprise Landing Zone

> **Work in progress.** This guide is incomplete and will change. The Department for Education (DfE) team that runs
> Explore Education Statistics (EES) is recording each step as it carries it out, while moving EES into the DfE
> Enterprise Landing Zone (ELZ). Steps may be missing, in the wrong order, or replaced as we learn more. When you carry
> out a step, or find one that's wrong, update this guide in the same change.

This guide covers the one-off setup in Azure DevOps and Azure that an environment needs before its infrastructure
pipelines can run. After that, run the pipelines in the order given in "Deploying a new environment" in
[README.md](README.md).

Related documents:

- [azure-resource-update-plan-for-elz.md](azure-resource-update-plan-for-elz.md): the plan for the move to the ELZ, with
  its decisions, open questions and the status of each action.
- [README.md](README.md): the order to run the infrastructure pipelines in, and things that have caught us out.

## 1. Before you start

### Your Azure environment may be different

This repository is open source, and this guide is written from DfE's experience. Every organisation's Azure set-up is
different, so treat the steps as a starting point rather than a recipe.

The templates support two set-ups, chosen by the `elz` variable (see [section 4](#4-create-the-variable-groups)):

- **Legacy**: the subscriptions DfE has run EES in until now.
- **ELZ**: DfE's Enterprise Landing Zone, a centrally managed Azure platform with its own policies and shared services.
  [Section 2](#2-what-the-elz-doesnt-let-us-do) lists the constraints it puts on EES.

Your subscriptions may have neither set of constraints, or different ones: other policies, required tags, naming rules,
network address ranges, DNS, and limits on which roles your pipelines can assign. Check these with whoever runs your
Azure platform before you start. The places in the templates you're most likely to need to change are:

- the param files in each template's `parameters/` and `parameters/elz/` folders;
- the resource names in `templates/common/resource-names.bicep`;
- the tags in `resourceTags` (see [section 4](#4-create-the-variable-groups)).

If you don't use Azure, the templates and pipelines here won't apply. They're written in Bicep for Azure Resource
Manager and run in Azure DevOps.

### Keep your environment's details in private setup notes

This guide leaves out values specific to any one deployment, such as subscription IDs, resource name prefixes, tag
values, group names and URLs. The DfE EES team keeps its own in a private setup document. If you're setting up EES from
this repository, we suggest you do the same, rather than committing them.

Your private setup notes should record:

- **Subscriptions:** the name and ID of each environment's subscription, and its resource group.
- **Resource name prefix** for each environment (the `subscription` variable in [section 4](#4-create-the-variable-groups)).
- **Network:** the address ranges allocated to each environment's VNet.
- **Azure DevOps:** the name of each environment's service connection.
- **Entra ID groups:** the name, object ID and portal link of the group that administers the SQL Servers.
- **Tags:** the values of every tag in `resourceTags`, including any your platform makes mandatory.
- **App Registrations:** the name and client ID of each one in [section 5](#5-app-registrations), for each environment.
- **Public API private DNS:** the Container App Environment's domain and IP address, and who you asked to create its
  records (see [section 6](#6-run-the-infrastructure-pipelines)).
- **Contacts:** who to ask on your Azure platform team, for example for IP ranges, DNS records and role assignments.

## 2. What the ELZ doesn't let us do

These are the ELZ constraints we know of so far, and how the templates handle them. Add to this list as we find more.

| Constraint | What it means for EES | How we handle it |
|---|---|---|
| **Mandatory tags.** Resources need the tags the ELZ requires, including `Service Offering` and `Product`. | Every resource the pipelines create must carry them. | Add them to `resourceTags` in the `Common` variable group ([section 4](#4-create-the-variable-groups)). The Core, Public API, Screener, Search and Analytics pipelines pass `resourceTags` to their templates. **Known gap:** the Main infrastructure pipeline (`bicep-main-infrastructure-release`) doesn't take `resourceTags` yet, so its resources don't get these tags (action 27 in the ELZ plan). |
| **No private DNS zones.** The DfE networking team manages private DNS zones centrally, and we can't create our own. | The templates can't create the `privatelink` zones that private endpoints register in, or the private zone the Public API Container App Environment needs. | With `elz` on, the templates don't create private DNS zones, records or DNS zone groups. We ask the networking team for the Public API's records (see "Requesting the Public API's private DNS records in the ELZ" in [README.md](README.md)). [Section 7.1 of the ELZ plan](azure-resource-update-plan-for-elz.md#71-private-dns) lists every change. |
| **IP address ranges are allocated by the platform team.** | We can't choose our own VNet and subnet ranges, and the ranges we're given must fit our subnet layout. | The ranges go in the core `parameters/elz/` param files. [Section 5 of the ELZ plan](azure-resource-update-plan-for-elz.md#5-subnet-migration) has the subnet layout and the size of range it needs. |

Still to find out (see [Open questions](azure-resource-update-plan-for-elz.md#8-open-questions) in the ELZ plan): which roles our pipelines' service principal can assign, how
traffic is routed through the hub network, and whether policy requires NSGs or particular subnets.

## 3. Import the core infrastructure pipeline

1. In Azure DevOps, go to **Pipelines** > **Create Pipeline**.
2. Select **Git**, then this repository.
3. Select the existing YAML file `infrastructure/templates/core/ci/azure-pipelines.yml`.
4. Name the pipeline "Core infrastructure deploy".

The other infrastructure pipelines will be imported the same way. Add them here as we do them.

## 4. Create the variable groups

The infrastructure pipelines read their settings from variable groups in **Pipelines** > **Library**. Each pipeline
uses the shared `Common` group, plus one group per environment named after the deploy stage's environment: `Common -
Dev`, `Common - Test`, `Common - Pre-Prod` and `Common - Prod`.

### Common

| Variable | What it holds |
|---|---|
| `serviceConnectionDev` | The name of the Azure DevOps service connection for the Dev subscription |
| `serviceConnectionTest` | The same, for Test |
| `serviceConnectionPreProd` | The same, for Pre-Prod |
| `serviceConnectionProd` | The same, for Prod |
| `sqlAdministratorLogin` | The admin user of the Core and Public SQL Servers |
| `sqlAzureAdministratorLogin` | The name of the Entra ID group that administers the SQL Servers |
| `sqlAzureAdministratorSid` | The object ID of that Entra ID group |
| `teamEmailAddresses` | Email addresses to notify of SQL security alerts and vulnerability assessment scans, as a JSON array |
| `maintenanceFirewallRules` | IP ranges allowed in for maintenance, as a JSON array of `{"name": ..., "cidr": ...}` objects |
| `resourceTags` | The tags to put on every resource, as a JSON object (see below) |

`resourceTags` has `CostCentre`, `Department`, `Solution`, `ServiceOwner`, `CreatedBy` and `DeploymentRepoUrl`. In the
ELZ, also add the mandatory tags, such as `Service Offering` and `Product` ([section 2](#2-what-the-elz-doesnt-let-us-do)).
Extra tags are passed through to the resources as they are.

### Common - Dev, Common - Test, and so on

| Variable | What it holds |
|---|---|
| `resourceGroupName` | The environment's resource group |
| `subscription` | The environment's prefix for resource names |
| `subscriptionId` | The environment's subscription ID |
| `publicSiteUrl` | The public site's URL, used by Front Door |
| `elz` | `true` for an ELZ environment. This switches the pipelines to the ELZ param files (`parameters/elz/`) and resource names. Anything else, or leaving it out, means legacy |

DfE has created `Common - Dev` and `Common - Test` so far. `Common - Pre-Prod` and `Common - Prod` are still to do.

### EES - Build

The build pipeline (`azure-pipelines-main.yml`) uses the `EES - Build` group. For the ELZ, set `elz` to `true` in it, so
that the .NET apps are built for Linux, which is what the ELZ App Services run on.

## 5. App registrations

EES uses these App Registrations in Entra ID, one of each per environment. App Registrations belong to the Entra ID
tenant, not to a subscription, so if your new environments are in the same tenant as your old ones, you can reuse the
existing App Registrations. DfE is reusing its own.

Their details go into the environment's Key Vault by hand, once the Core pipeline has created the Key Vault. No pipeline
writes these secrets. The secret names below are the ELZ ones. The legacy names are in `templates/common/resource-names.bicep`.

| App Registration | What it's for | Key Vault secrets (ELZ names) |
|---|---|---|
| Admin | The Admin single-page app signs users in with it | `admin-openidconnect-client-id` |
| Admin API | Exposes the `access-admin-api` scope that the Admin single-page app asks for when calling the Admin API, and is the audience Admin checks for on those tokens | `admin-openidconnect-valid-audience`, `admin-openidconnect-fully-qualified-scope-name` |
| Public API | Protects the Public API Container App with Entra ID authentication. It needs an `Admin.Access` app role (see below) | `papi-app-registration-client-id` |
| Public API Data Processor | Protects the Data Processor Function App with Entra ID authentication | `papiprocessor-app-registration-client-id` |
| Screener | Protects the Screener Function App with Entra ID authentication | `screener-app-registration-client-id` |

Admin also needs `admin-openidconnect-authority` and `admin-openidconnect-valid-issuers`, which identify the Entra ID
tenant that signs its users in, rather than any one App Registration.

### The Public API's `Admin.Access` app role

Admin calls the Public API for privileged actions, such as seeing draft API data set versions that aren't published yet.
The Public API only allows these when Admin's access token carries the `Admin.Access` app role.

1. On the Public API App Registration, create an app role with the value `Admin.Access`, and allow it to be assigned to
   applications. If you're reusing an existing App Registration, check that the role is already there.
2. Assign the role to the identity Admin calls the Public API with, once that identity exists. Admin gets its token with
   its managed identity, not with the Admin App Registration, so the role goes on the managed identity's service
   principal. In the ELZ that's Admin's user-assigned identity, which the Main infrastructure pipeline creates. Legacy
   environments assigned it to Admin's system-assigned identity.

Admin can then request access tokens for the Public API with the role in them. Nothing fails at deploy time without the
role, but Admin gets not found or forbidden responses for draft API data set versions. No pipeline assigns it: see
"Giving Admin the Public API's `Admin.Access` app role" in [README.md](README.md) for the commands.

## 6. Run the infrastructure pipelines

Follow "Deploying a new environment" in [README.md](README.md). In the ELZ, also see "Requesting the Public API's private
DNS records in the ELZ" there, as we can't create private DNS zones in the ELZ and have to ask the DfE networking team
for them.
