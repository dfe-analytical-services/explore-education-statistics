# HTTP status alert thresholds

EES-7664: configurable Http401 and Http5xx thresholds for team review. No
production thresholds are selected by this change. Existing HTTP rules and
failed-request, availability, job, completion and dependency monitoring remain
in place.

## What to configure

The main infrastructure release accepts `http5xxAlerts` and `http401Alerts`.
See [typed examples](../../../bicep-main-infrastructure-release/parameters/alerts-example.bicepparam).
Do **not** deploy the example file: its illustrative overrides are not a
deployment recommendation.

`http5xxAlerts` independently overrides the existing rule for `publicSite`,
`dataApi` (Statistics/Data API), `contentApi`, `admin`, `importer`, `publisher`
and `notifier`. An omitted entry preserves `Minimum > 0`, 15-minute window,
five-minute evaluation and Warning severity. Dynamic failed-request rules are
unaffected. Existing rule names are retained, so approved overrides update a rule
instead of adding a duplicate.

`http401Alerts` takes the same keys and shape, and overrides the existing Http401
rule independently of Http5xx. Some 401s are routine (e.g. expired Admin tokens and
silent reauthentication), so a raised count may suit Admin. A 401 count is not a
measure of users unable to sign in. An unexpected 401 rise on a public service can
still indicate an auth misconfiguration.
The containerised Public API data processor and screener function apps keep their
existing Http401/Http5xx rules; they are deployed separately and are not covered.

For a count policy, use `Total`: `threshold: 4` means **more than four**, i.e.
five errors in the window. `Minimum > 0` is not the same as a total error count:
it tests the minimum metric value returned for that window. Windows overlap;
evaluation frequency is how often Azure checks, not a separate counting window.

## Team decisions and rollout

For each service record: owner, evidence and normal traffic, count limit, window,
severity, expected response, acceptable delay and rollback. The public site may
need a sensitive count; Content/general Admin thresholds require evidence before
relaxation. Importer, publisher and notifications must retain failed-job/completion
signals.

1. Reconcile deployed rules with source. Compare old/new coverage, aggregation,
   windows and response expectations before consolidating anything.
2. Compile the main Bicep and example parameter file.
3. Agree values with the team, copy the `http5xxAlerts` / `http401Alerts` blocks
   into the environment's `main-<env>.bicepparam`, starting with a non-production
   environment.
4. Verify firing **and recovery**, rule description and delivery to both Slack
   and Teams. Local compilation is not live delivery validation.
5. Compare two complete weeks of actionable detection and delay, not message
   reduction alone. Replay the September/October milestone cases.
6. Roll back an override by removing its entry and redeploying.

References: [metric alert schema](https://learn.microsoft.com/en-us/azure/templates/microsoft.insights/2018-03-01/metricalerts),
[App Service metrics](https://learn.microsoft.com/en-us/azure/app-service/web-sites-monitor).
