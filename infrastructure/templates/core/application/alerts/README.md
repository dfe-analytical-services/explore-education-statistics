# Alerts infrastructure

## Overview

This infrastructure area supports the relaying of metric alerts, including Data Factory activity failures, to
Teams and Slack channels.

Metric alerts are linked to an Action Group, which in turn is linked to a Logic App. The metric alert mechanism
sends a JSON payload to the Logic App, and then the Logic App handles converting that JSON into human-readable
messages that are then posted to Teams and Slack.

The [alerts.bicep](alerts.bicep) file is responsible for putting this all together.

## Logic app

The [Logic App](alerts-logic-app.bicep) receives JSON payloads from metric alerts. It captures important
information using a series of `Compose` actions, and then 2 HTTP actions, one for Teams and one for
Slack, take those variables and construct POSTs in the correct format for their target platforms.

All Slack configuration lives in a single `ees-alerts-slackconfig` Key Vault secret, holding a JSON
array of workspaces:

```json
[
  { "channels": ["C067Z1K68UD"], "authToken": "xoxb-..." },
  { "channels": ["C0C13TPGB53"], "authToken": "xoxb-..." }
]
```

Each workspace carries the app token that can post to its own channels, so channels in different
Slack workspaces can be mixed without the template knowing anything about who owns them. Nothing
about Slack is configured in the `.bicepparam` files - adding, removing or retargeting a channel is
a Key Vault edit, not a deployment.

`Post to Slack workspaces` loops over that array and `Post to channels` loops over the channels
within each entry, so the token used for a POST is always the one belonging to the workspace the
channel came from. Nested loops are supported up to an action nesting depth of 8; iterations of a
nested loop always run sequentially.

Because the token now flows through `items()` rather than being referenced directly as a
`securestring` parameter, the HTTP action sets `secureData` on its inputs and outputs so the token is
not recorded in run history.

Every app needs the `chat:write` scope and must be invited to its channels, otherwise
`chat.postMessage` returns HTTP 200 with `"ok": false`. Because that is not an HTTP failure, the
`Record rejected Slack post` condition inspects the response body and collects any rejected channel
into the `slackFailures` variable, and `Fail if any Slack post was rejected` then terminates the run
as failed - so dropped alerts surface in the `WorkflowRuntime` diagnostic logs instead of passing
silently. Both loops run at a concurrency of 1 because appending to a variable from parallel
iterations is not safe, and `Terminate` is not permitted inside a `Foreach`, which is why the failure
is raised after the loops rather than within them.

The [Logic App definition](alerts-logic-app-definition.json) defines the workflow.

## Action Group

The [Action Group](alerts-action-group.bicep) is just link between metric alerts, including Data Factory
activity failures, and the Logic App. The metric alert mechanism calls the Action Group, and the Action
Group forwards the messages to the Logic App.

## Testing the mechanism

### Via the Action Group

Generic tests can be run directly from the Action Group in Azure Portal.

1. Visit the Action Group in Azure Portal.
2. Click "Test".
3. Select either `Metric alert - Dynamic threshold` or `Metric alert - Static threshold` and click "Test".
4. Check in Teams and Slack for the generated messages.

### Via the Logic App

More low-level tests can be run from the Logic App itself using custom JSON payloads.

1. Visit the Logic App in Azure Portal.
2. On the Overview page, click the "Run" dropdown at the top and select "Run with payload".
3. Enter a test JSON payload in the "Body" textarea.
4. Click "Run".
5. Check in Teams and Slack for the generated messages.

Some example JSON payloads are available in the [test data folder](test-data).