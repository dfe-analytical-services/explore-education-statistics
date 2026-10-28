# Alerts infrastructure

## Overview

This infrastructure area supports the relaying of metric alerts, including Data Factory activity failures, to
a Teams channel.

Metric alerts are linked to an Action Group, which in turn is linked to a Logic App. The metric alert mechanism
sends a JSON payload to the Logic App, and then the Logic App handles converting that JSON into a human-readable
message that is then posted to Teams.

The [alerts.bicep](alerts.bicep) file is responsible for putting this all together.

## Logic app

The [Logic App](alerts-logic-app.bicep) receives JSON payloads from metric alerts. It captures important
information using a series of `Compose` actions, and then the `Post message to Teams` HTTP action uses those
values to build an Adaptive Card and POST it to a Power Automate webhook, which posts the card to Teams.

The webhook URL is held in the `ees-alerts-teamswebhookurl` Key Vault secret. It is read at deployment time,
so to retarget the alerts, update the secret and redeploy the core infrastructure for that environment.

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
4. Check in Teams for the generated message.

### Via the Logic App

More low-level tests can be run from the Logic App itself using custom JSON payloads.

1. Visit the Logic App in Azure Portal.
2. On the Overview page, click the "Run" dropdown at the top and select "Run with payload".
3. Enter a test JSON payload in the "Body" textarea.
4. Click "Run".
5. Check in Teams for the generated message.

Some example JSON payloads are available in the [test data folder](test-data).
