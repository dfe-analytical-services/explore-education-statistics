# Notifier Functions

This project contains Azure Functions responsible for managing subscriptions to publications, and notifying users of changes to their subscribed publications.

It uses [GOV.UK Notify](https://www.notifications.service.gov.uk) to send notifications by email.

## Local development and testing

To test sending emails when running the Azure Functions app locally, you will need to configure the GOV.UK Notify API key and the email template ids. These are kept as secrets and should not be committed to the source code repository.

Create a copy of the `appsettings.Local.json.example` as `appsettings.Local.json`.

```bash
cp src/GovUk.Education.ExploreEducationStatistics.Notifier/appsettings.Local.json.example src/GovUk.Education.ExploreEducationStatistics.Notifier/appsettings.Local.json
```

`appsettings.Local.json` is already in the `.gitignore` file, so it should remain as untracked.

Update the `GovUkNotify` section of the `appsettings.Local.json` file by substituting values `change-me` with values for the `ApiKey` and each of the email templates ids.

A member of the EES team will need to invite you to be a team member of the GOV.UK Notify service and generate you an API key.

As part of the GOV.UK Notify team you will be able to see the dashboard where you can create and edit templates.

API keys that have been generated for testing locally are usually limited to sending emails to recipients who are GOV.UK Notify team members (including yourself) or configured in a guest list.

## Altering existing GOV.UK Notify templates

**IMPORTANT**: Existing templates should not be edited directly except for very minor content changes as they are used by all environments including Production.

To alter an existing template, first copy its Subject and Message into a new template.

Reference the new template id in `appsettings.Local.json` to test it locally.

When the changes are approved this new template id can be set in the Azure Function app configuration as it is promoted through the different Azure environments.

When the new template id has reached Production and the old template is no longer in use, tidy up by deleting the old template.
