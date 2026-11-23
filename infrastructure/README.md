# EES infrastructure

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
