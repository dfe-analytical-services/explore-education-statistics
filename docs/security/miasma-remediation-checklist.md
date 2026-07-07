# Miasma remediation checklist

This checklist records the EES investigation and remediation actions for the
Miasma supply-chain incident described by Microsoft:

https://www.microsoft.com/en-us/security/blog/2026/06/02/preinstall-persistence-inside-red-hat-npm-miasma-credential-stealing-campaign/

Use 1 May 2026 to 5 June 2026 as the conservative exposure window, plus any
later builds from agents that have not been verified clean.

## Scope

In scope for the EES team:

- Dependency evidence checks in this repository.
- Azure DevOps pipeline, pool, and service connection inventory.
- PAT/feed credential review for NuGet and GitHub Packages.
- Raising remediation tasks for platform or security-owned actions.

Needs platform/security access:

- Self-hosted Azure DevOps agent audit or rebuild.
- Entra ID service principal sign-in review and secret/certificate rotation.
- Key Vault secret rotation where exposure cannot be ruled out.
- Endpoint forensics on developer machines.

Out of scope for EES alone:

- DfE-wide guidance.
- Microsoft confirmation of third-party reporting about disabled Microsoft repos.
- Full estate-wide endpoint investigation.

## Evidence recorded from the repo

Completed local checks:

- No direct `@redhat-cloud-services` package references were found in package
  manifests or lockfiles.
- No direct Python `durabletask` package reference was found in `Pipfile` or
  `Pipfile.lock`.
- No GitHub `Azure/functions-action` reference was found.
- No direct `@azure/functions` npm package reference was found.
- The root package lifecycle hook is `preinstall: pnpm check:node`.
- EES uses Azure Functions and Durable Task via .NET package references.
- Azure DevOps deployments use `AzureCLI@2`, `AzurePowerShell@5`, and
  `az functionapp deployment source config-zip` rather than GitHub Actions
  Azure Functions deployment actions.
- Named Azure DevOps pools `ees-ubuntu2204-large` and `ees-ubuntu2204-xlarge`
  appear in EES pipelines and should be treated as private/self-hosted until
  Azure DevOps confirms otherwise.

Tracking status:

| Area | Status | Next action |
| --- | --- | --- |
| Dependency indicator scan | Complete from repo evidence | Re-run if lockfiles change |
| Azure Functions GitHub Action check | Complete from repo evidence | No action unless pipeline model changes |
| Azure DevOps service connection inventory | In progress | Resolve variable group values in Azure DevOps |
| NuGet/GitHub Packages PAT review | In progress | Confirm scope, owner, expiry, and runner usage |
| ACR service connection review | Not started | Resolve `AcrServiceConnection` and inspect backing auth |
| Azure Resource Manager service connection review | Not started | Resolve environment service connection variables |
| Self-hosted agent audit | Needs platform/security owner | Raise task for `ees-ubuntu2204-large` and `ees-ubuntu2204-xlarge` |
| Artifact rebuild/redeploy | Conditional | Only needed if agent exposure is confirmed or cannot be ruled out |

Evidence commands:

```powershell
rg -n "@redhat-cloud-services|durabletask|Azure/functions-action|functions-action|@azure/functions|Miasma|_index\.js" -S -g "!node_modules/**" -g "!**/node_modules/**" -g "package.json" -g "pnpm-lock.yaml" -g "Pipfile" -g "Pipfile.lock" -g "*.csproj" .

rg -n "pool:|vmImage:|NuGetAuthenticate@1|nuGetServiceConnections|Docker@2|containerRegistry|AzureCLI@2|AzurePowerShell@5|azureSubscription|serviceConnection|AcrServiceConnection|NuGetServiceConnectionName|SPN_NAME" -S -g "*.yml" -g "*.yaml" .
```

## Azure DevOps credential inventory

Resolve these pipeline variables in Azure DevOps variable groups and record the
actual service connection display names:

- `NuGetServiceConnectionName`
- `AcrServiceConnection`
- `serviceConnectionDev`
- `serviceConnectionTest`
- `serviceConnectionPreProd`
- `serviceConnectionProd`
- `SPN_NAME`

For each resolved service connection, capture:

| Field | Value |
| --- | --- |
| Display name | |
| Service connection type | |
| Auth scheme | |
| Subscription/resource | |
| Tenant ID | |
| Client/app ID | |
| Backing credential type | Secret / certificate / PAT / workload identity federation / managed identity |
| Secret expiry | |
| Owner | |
| Used by pipelines | |
| Used by self-hosted agents | Yes / No / Unknown |
| Rotation required | Yes / No |
| Reason | |
| Rotation completed date | |
| Validation run | |

Rotation decision:

- Rotate long-lived secrets, certificates, PATs, or admin credentials where
  exposure cannot be ruled out.
- Rotate GitHub Packages or NuGet PATs if they are broad-scoped, write-capable,
  long-lived, used on self-hosted agents, or their usage location is unknown.
- Review sign-ins and role assignments for workload identity federation or
  managed identity. There may be no secret to rotate.
- Rotate Key Vault secrets only if a service connection, agent, deployment
  identity, or runtime has credible exposure.

## Self-hosted agent audit task

Raise a platform/security task for these pools:

- `ees-ubuntu2204-large`
- `ees-ubuntu2204-xlarge`

Required outcome:

- Confirm whether each pool is self-hosted/private or Microsoft-hosted.
- Identify agents that ran EES jobs during the exposure window.
- Rebuild affected agents from known-clean images, or provide written evidence
  of an equivalent audit and cleanup.
- Inspect or clear credential and dependency caches:
  - `~/.azure`
  - `~/.docker`
  - `~/.npmrc`
  - `~/.nuget/NuGet/NuGet.Config`
  - `~/.nuget/packages`
  - GitHub CLI auth cache
  - Azure DevOps agent workspaces
  - npm/pnpm caches
- Hunt for:
  - suspicious Bun execution
  - unexpected npm lifecycle script execution
  - unexpected outbound connections
  - unexpected public GitHub repositories with Miasma markers
  - files or repositories with `Miasma: The Spreading Blight`

Preferred remediation is agent rebuild rather than cache deletion alone.

## Artifact remediation

Only perform this if a self-hosted agent is confirmed or suspected exposed:

- Treat artifacts built on that agent during the exposure window as untrusted.
- Rebuild Function App packages and Docker images from a verified clean agent.
- Redeploy clean artifacts.
- Do not purge production artifacts until clean replacements are available and
  deployment owners confirm the rollback path.

## Validation

After each credential rotation:

- Run a NuGet restore/build path that uses `NuGetAuthenticate@1`.
- Run the smallest Docker build/push path that uses `AcrServiceConnection`.
- Run a non-production infrastructure deploy using the relevant Azure Resource
  Manager service connection.
- Capture the old credential revoked date, new credential created date, scope,
  expiry, owner, and validation run link.
