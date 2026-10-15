# Service and journey alert thresholds

EES-7664 / EES-7665: configurable thresholds for team review. No production
thresholds are selected by this change. Existing HTTP rules and failed-request,
availability, job, completion and dependency monitoring remain in place.

## What to configure

The main infrastructure release accepts `http5xxAlerts` and `journeyAlerts`.
See [typed examples](../../../bicep-main-infrastructure-release/parameters/alerts-example.bicepparam).
Do **not** deploy the example file: it contains placeholder resource IDs and
illustrative thresholds. Journey examples are disabled; the illustrative metric
override is not a deployment recommendation.

`http5xxAlerts` independently overrides the existing rule for `publicSite`,
`dataApi` (Statistics/Data API), `contentApi`, `admin`, `importer`, `publisher`
and `notifier`. An omitted entry preserves `Minimum > 0`, 15-minute window,
five-minute evaluation and Warning severity. HTTP 401 and dynamic failed-request
rules are unaffected. Existing rule names are retained, so approved overrides
update a rule instead of adding a duplicate.

For a count policy, use `Total`: `threshold: 4` means **more than four**, i.e.
five errors in the window. `Minimum > 0` is not the same as a total error count:
it tests the minimum metric value returned for that window. Windows overlap;
evaluation frequency is how often Azure checks, not a separate counting window.

`journeyAlerts` adds independent log alerts for any service emitting AppRequests
to the selected workspace, including the separate Public Data API. Each policy
requires an explicit Application Insights resource ID and exact role. Optional
path and request-name lists narrow the cohort. Values within a list are ORed;
the two lists are ANDed. Empty lists select all requests for that component/role.
All path comparisons are exact and exclude the query string.

| Mode | Condition within the selected window |
| --- | --- |
| `count` | Errors >= `errorCountThreshold`, even with little traffic |
| `rate` | Errors / requests >= `errorRateBasisPoints` / 10,000, AND requests >= `minimumRequests`, AND errors >= `minimumErrors` |
| `either` | Either of the above conditions |

For example, count >= 10 **or** rate >= 5% with at least 20 requests and three
errors catches 3/20 by rate and 10/10,000 by count. It does not catch 1/1. For an
important low-volume journey, a separate count >= 1 policy can catch 1/1.
Exactly matching a threshold fires these log alerts; the older metric rule uses
strictly greater than. Log rules evaluate every five minutes, plus ingestion
and alert-processing delay. `Breach = 1` is a policy result, **not one HTTP error**;
open the alert's query results for Requests, Errors and ErrorRateBasisPoints.

## Public journeys and specific statistics

The public site has highest priority. Keep broad platform/availability detection
while validating journey coverage; a quiet or missing request stream cannot
prove the site is healthy. The example covers `/data-tables` on the public site
and an important saved query on the Statistics API via
`/api/tablebuilder/fast-track/{dataBlockId}`. The latter is defined in
`src/GovUk.Education.ExploreEducationStatistics.Data.Api/Controllers/TableBuilderController.cs`.
Use the **actual saved block ID** chosen by the team. These are two separate
signals within a journey; timing alone does not establish a shared incident.

Arbitrary POST `/api/tablebuilder` queries cannot be distinguished by URL because
their filters are in the request body. Do not log bodies or sensitive query
values to work around this. Start with endpoint-level monitoring; distinguishing
a particular POST query needs a separately reviewed, allowlisted scenario ID in
server telemetry. HTTP monitoring also cannot detect incorrect 200 responses,
browser rendering failures or users who never reach the application. Retain
availability tests and agree an authenticated/synthetic journey test for those gaps.

Before enabling a rate rule, confirm the numerator and denominator represent the
same production request population. The query filters both identically and
applies ItemCount sampling weights to both. Multiple Next.js request/rendering
spans can still inflate counts: select the observed **outer HTTP request Name**
and role, and compare to platform totals. Do not deploy a site-wide log rate rule
until this is resolved. Sampled estimates are not an exact failed-request census.
No user-agent filtering is included: automated Public Data API consumption may
be legitimate. Content bot explanations still require evidence.

## Admin users unable to sign in

The browser emits `AdminSignInOutcome` at identity-provider, token, Admin API,
permissions or completion stages. An opaque attempt ID deduplicates terminal
events. No account identifier, token, raw error or request body is added to the
event. Missing/expired invitations, cancelled login and routine silent-token
reauthentication are expected outcomes and excluded from paging.
Events that finish before Application Insights initialisation are buffered in
memory (up to 100) and drained once it is ready. Failures request a flush. A page
closing before initialisation can still lose the buffered event.

`adminSignIn` policies count observed failed attempts for:

- Users accepted by the Admin sign-in API during the current attempt, including
  a subsequent permissions failure.
- The **same account** previously accepted within the last 24 hours in this tab,
  including a token/API failure before it can be accepted again. Its account ID
  and timestamp stay in sessionStorage and are not sent in the custom event.
  Access denial clears that marker; storage failure falls back to unverified.

An identity-provider login alone does not prove Admin access. Unverified failures
are recorded for investigation but excluded from this dedicated policy. The
companion `/api/sign-in` server-error example catches first-time invited users'
server failures as well. Do not describe these browser signals as a complete or
authoritative census of legitimate users: fresh browsers, identity-provider
outages before callback, cancelled/abandoned redirects, blocked telemetry,
sampling and lost network connections can escape them. Correlate with identity
provider and server logs; preserve availability monitoring. Both explicit login
and existing-session validation on page load pass through this flow.

General Admin threshold changes cannot disable the dedicated sign-in rule.
Before reducing general Admin noise, deploy telemetry, verify real success and
failure events, and enable the agreed sign-in policy. The query counts distinct
observed attempts, without extrapolating sampled events into invented attempts.

## Team decisions and rollout

For each service/journey record: owner, critical user action, evidence and normal
traffic, count limit, rate and traffic floor, window, severity, expected response,
acceptable delay and rollback. Public site and important low-volume queries may
need sensitive counts; Content/general Admin thresholds require evidence before
relaxation. Public Data API consumer failures remain important even when automated.
Importer, publisher and notifications must retain failed-job/completion signals.

1. Reconcile deployed rules with source and confirm component/role/path names.
   Compare old/new coverage, aggregation, windows, missing-data behaviour and
   response expectations before consolidating anything.
2. Compile the main Bicep and example parameter file. Deploy telemetry and the
   notification Logic App update through their respective normal releases first.
   The Logic App accepts Log Alerts V2's `metricMeasureColumn` as well as metricName.
3. In a non-production workspace, run each generated KQL query against known
   requests. Check 0/0, 1/1, just below/exactly/above thresholds, 3/20, 10/10,000,
   out-of-cohort errors, sampled records, duplicate spans and no matching role.
   Check sign-in success, known-user 401/network/permissions failures, unknown
   account failures, expired/no invite, normal token expiry and cancellation.
4. Verify firing **and recovery**, rule description, query link and delivery to
   both Slack and Teams. Reuse the Log Alerts V2 fixtures in the core alert
   test-data folder. Local compilation is not live query/delivery validation.
5. Agree values with the team, copy policies into the environment parameter file
   and enable them. Compare two complete weeks of actionable detection and delay,
   not message reduction alone. Replay the September/October milestone cases.
6. Roll back a metric override by removing its entry and redeploying. Disable a
   new log rule by retaining it with `enabled: false` and redeploying; merely
   removing an entry in incremental deployment does **not** delete the resource.

References: [scheduled query rule schema](https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/2023-12-01/scheduledqueryrules),
[Azure common alert schema](https://learn.microsoft.com/en-us/azure/azure-monitor/alerts/alerts-common-schema),
[AppRequests fields](https://learn.microsoft.com/en-us/azure/azure-monitor/reference/tables/apprequests),
[AppEvents fields](https://learn.microsoft.com/en-us/azure/azure-monitor/reference/tables/appevents).
