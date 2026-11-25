# EES infrastructure

The move to the DfE Enterprise Landing Zone (ELZ) is planned and tracked in
[azure-resource-update-plan-for-elz.md](../azure-resource-update-plan-for-elz.md): naming, network layout, decisions,
and the status of each action.

## Deploying a new environment

In a new environment, the infrastructure pipelines must first run in this order, as each uses resources that earlier
ones create, such as file shares to mount and the main apps' user-assigned identities:

1. Core, with Front Door (and Data Factory) but not Application Gateway.
2. Analytics and Search, in either order.
3. Public API, with the PostgreSQL Flexible Server but not the Container App or Data Processor, which allow calls from
   Admin's identity. Then add the PostgreSQL users manually.
4. Main (`bicep-main-infrastructure-release`), which mounts the Analytics and Public API file shares, and creates
   Admin's user-assigned identity. Then give Admin's identity the Public API's `Admin.Access` app role manually (see
   below).
5. Public API again, with the Container App and Data Processor.
6. Screener, Data Factory pipelines, and Core again with Application Gateway, which routes to the Public API Container
   App.

Each pipeline's `ci/azure-pipelines.yml` says which of these steps it covers.

### Giving Admin the Public API's `Admin.Access` app role

Admin calls the Public API with its managed identity, and the Public API only shows Admin unpublished data when the
token carries the `Admin.Access` app role (see `AuthorizationHandlerService`). In ELZ, Admin uses its user-assigned
identity (`resourceNames.admin.identity`), so that identity needs the role. Legacy environments assigned it to Admin's
system-assigned identity instead.

Without the role, nothing fails at deploy time, but Admin gets not found or forbidden responses for draft API data set
versions.

No pipeline assigns the role. The `assign-app-role-to-service-principal.yml` step in
`public-api/ci/jobs/deploy-infrastructure.yml` is commented out, and only looks up system-assigned identities. Running
it from a pipeline would also need the service connection to have Microsoft Graph permission to assign app roles.

Assign it once per environment, as someone allowed to assign app roles on the Public API's app registration:

```bash
# Admin's user-assigned identity, and the Public API's app registration (service principal) and its role.
adminPrincipalId=$(az identity show --resource-group <resource group> --name <Admin identity name> \
  --query principalId --output tsv)
publicApiSpId=$(az ad sp list --display-name <Public API app registration name> --query "[0].id" --output tsv)
appRoleId=$(az ad sp show --id "$publicApiSpId" --query "appRoles[?value=='Admin.Access'].id" --output tsv)

az rest --method POST \
  --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$adminPrincipalId/appRoleAssignments" \
  --body "{\"principalId\": \"$adminPrincipalId\", \"resourceId\": \"$publicApiSpId\", \"appRoleId\": \"$appRoleId\"}"
```

See [Assign a managed identity access to an application role](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/assign-app-role-managed-identity-azure-cli).

## App settings

The infrastructure pipelines and the application release pipelines both write App Service and Function App app
settings, so each has to keep what the other set:

- The application release pipelines work out each app's settings from the Bicep in `main-app-release/appsettings`, and
  apply them with `az webapp config appsettings set`. That only adds or updates the settings it's given, and leaves the
  rest alone.
- The infrastructure pipelines first read each app's existing settings (`common/ci/tasks/get-current-appsettings.yml`),
  and pass them into the templates to be merged with the infrastructure's own settings (see `app-service.bicep`).
  Without this, every infrastructure deploy would reset the application's settings.

Things to know:

- **Writing app settings restarts the app,** even when nothing has changed. So every infrastructure deploy restarts the
  apps it covers, and they're briefly unavailable while they start up.
- **Never write back the output of `az webapp config appsettings list`.** The Azure CLI deliberately returns
  `DOCKER_REGISTRY_SERVER_PASSWORD` as `null`, so writing its output back blanks the password. The public site then
  can't pull its container image when it restarts. `get-current-appsettings.yml` calls the ARM
  `config/appsettings/list` action with `az rest` instead, which returns every value as stored.
- **Secrets are Key Vault references to a specific secret version** (see `secretRefsFromSecrets` in
  `common/functions.bicep`). When a secret changes in Key Vault, apps keep using the old version until the next
  application release updates the reference.

## App Service deploy slots

- **Basic plans don't support deploy slots.** The public site only gets its staging slot on other tiers. P0v3 is the
  cheapest Linux tier that supports them.
- **Managed identities don't swap.** In legacy environments, each slot has its own system-assigned identity, so a slot's
  identity needs the same role assignments as the production slot's (Key Vault, Search and so on). In ELZ environments,
  slots share the app's user-assigned identity instead.
- **A new slot has no application settings** until the next application release sets them. Until then it can't, for
  example, pull the public site's container image.

## Other things to know

- **Changing an App Service plan's tier** (for example Basic to PremiumV3) moves its apps onto new machines, so they
  restart.
- **Storage account properties left out of a deploy keep their current values.** Set properties such as
  `allowSharedKeyAccess` explicitly every time, so that the template, not a past deploy or manual change, decides them.
- **Free tier SignalR (`Free_F1`) allows only 20 concurrent connections.** By default, each app instance also opens 5
  server connections per hub, which count towards that limit.

## Checking a template change locally

`scripts/bicep-snapshot-diff` compares what two versions of the templates would deploy, without needing an Azure
login. See [its README](scripts/bicep-snapshot-diff/README.md).
