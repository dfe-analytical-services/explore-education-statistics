# Publisher Functions

This project contains Azure Functions responsible for publishing of approved Releases, their content and their files.

Releases can be scheduled to be published on a particular date or immediately on approval. There are 2 publisher flows covering these two scenarios.

## Forcing immediate publishing of scheduled Releases in test environments

All example HTTP requests below show examples of sending requests in a local development environment. Replace `http://localhost:7072` with the relevant environment's
Publisher Functions URL.

During manual or automated testing, it is handy to have a way to schedule Releases for publishing but to trigger that process to occur on demand, rather than having to wait for a lengthly
period before the scheduled Publisher Functions run. For this, we provide 2 Functions that can be triggered by HTTP requests; one stages scheduled Releases, whilst the other completes the 
publishing process for any staged Releases and makes them live.

By default, the two Functions for publishing scheduled Releases immediately will target only Releases scheduled for publication "today". However, passing an array of specific Release Ids
will affect only those specified Releases (and in addition will ignore their scheduled publishing date).

This first HTTP request will stage scheduled Releases with a scheduled publication date of "today":

```http request
POST http://localhost:7072/api/StageScheduledReleasesImmediately
Accept: 'application/json'
```

Alternatively to target specific Releases and ignore their scheduled dates:

```http request
POST http://localhost:7072/api/StageScheduledReleasesImmediately
Content-Type: 'application/json'

{
  "releaseIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

A JSON response of Release Ids that are now being staged will be returned:

```json
{
  "releaseIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

After a delay to allow the staging process to complete (the above request queues messages to stage Releases but does not wait for them to complete), this second request will complete the
publishing process of the staged Releases, returning an array of Release Ids to be published:

```http request
POST http://localhost:7072/api/PublishStagedReleaseContentImmediately
Accept: 'application/json'
```

Alternatively to target specific Releases:

```http request
POST http://localhost:7072/api/PublishStagedReleaseContentImmediately
Content-Type: 'application/json'

{
  releaseIds: ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

A JSON response of Release Ids that have now been published will be returned:

```json
{
  "releaseIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

After the HTTP request completes, the Releases will be published.

### Triggering Functions from CLI

cURL can be used to easily trigger the Functions from the command-line. Some examples below:

`curl http://localhost:7072/api/StageScheduledReleasesImmediately -X POST -H 'Accept: application/json'`

`curl http://localhost:7072/api/StageScheduledReleasesImmediately -X POST -H 'Content-Type: application/json' -d '{"releaseIds":["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]}'`

`curl http://localhost:7072/api/PublishStagedReleaseContentImmediately -X POST -H 'Accept: application/json'`

`curl http://localhost:7072/api/PublishStagedReleaseContentImmediately -X POST -H 'Content-Type: application/json' -d '{"releaseIds":["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]}'`
