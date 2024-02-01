# Notifier Functions

This project contains Azure Functions responsible for managing subscriptions to publications, and notifying users of changes to their subscribed publications.

The [GOV.UK Notify](https://www.notifications.service.gov.uk) service is used for sending notifications by e-mail.

## Local development and testing

Create a copy of the `appsettings.Local.json.example` file found in the root of the project directory and call it `appsettings.Local.json`.

*The new file should be automatically ignored by source control as it will contain sensitive information.*

To obtain a `NotifyApiKey`, a member of the EES team will need to invite you to the Gov.UK Notify service.

Once invited, you will also gain access to the various email templates used by the service, each with its own ID, which can be pasted into the `...TemplateId` sections marked `"change-me"` in the example JSON.

*IMPORTANT: The templates should **not** be edited for testing purposes, as they are used by the production application. To test a new or updated template, create a new template and reference it's ID in the JSON above.*
