# Publisher Functions

This project contains Azure Functions responsible for publishing of approved release versions, their content and their files.

Release versions can be scheduled to be published on a particular date or immediately on approval. There are two publisher flows covering these two scenarios.

## Forcing immediate publishing of scheduled release versions in test environments

All example HTTP requests below show examples of sending requests in a local development environment. Replace `http://localhost:7072` with the relevant environment's
Publisher Functions URL.

During manual or automated testing, it is handy to have a way to schedule release versions for publishing but to trigger that process to occur on demand, rather than having to wait for a lengthy
period before the scheduled Publisher Functions run. For this, we provide two Functions that can be triggered by HTTP requests; one stages scheduled release versions, whilst the other completes the 
publishing process for any staged release versions and makes them live.

By default, the two Functions for publishing scheduled release versions immediately will target only release versions scheduled for publication "today". However, passing an array of specific release version id's
will affect only those specified release versions (and in addition will ignore their scheduled publishing date).

This first HTTP request will stage scheduled release versions with a scheduled publication date of "today":

```http request
POST http://localhost:7072/api/StageScheduledReleaseVersionsImmediately
Accept: 'application/json'
```

Alternatively to target specific release versions and ignore their scheduled dates:

```http request
POST http://localhost:7072/api/StageScheduledReleaseVersionsImmediately
Content-Type: 'application/json'

{
  "releaseVersionIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

A JSON response of release version id's that are now being staged will be returned:

```json
{
  "releaseVersionIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

After a delay to allow the staging process to complete (the above request queues messages to stage release versions but does not wait for them to complete), this second request will complete the
publishing process of the staged release versions, returning an array of release version id's to be published:

```http request
POST http://localhost:7072/api/PublishStagedReleaseVersionContentImmediately
Accept: 'application/json'
```

Alternatively to target specific release versions:

```http request
POST http://localhost:7072/api/PublishStagedReleaseVersionContentImmediately
Content-Type: 'application/json'

{
  releaseVersionIds: ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

A JSON response of release version id's that have now been published will be returned:

```json
{
  "releaseVersionIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

After the HTTP request completes, the release versions will be published.

### Triggering Functions from CLI

cURL can be used to easily trigger the Functions from the command-line. Some examples below:

`curl http://localhost:7072/api/StageScheduledReleaseVersionsImmediately -X POST -H 'Accept: application/json'`

`curl http://localhost:7072/api/StageScheduledReleaseVersionsImmediately -X POST -H 'Content-Type: application/json' -d '{"releaseVersionIds":["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]}'`

`curl http://localhost:7072/api/PublishStagedReleaseVersionContentImmediately -X POST -H 'Accept: application/json'`

`curl http://localhost:7072/api/PublishStagedReleaseVersionContentImmediately -X POST -H 'Content-Type: application/json' -d '{"releaseVersionIds":["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]}'`
