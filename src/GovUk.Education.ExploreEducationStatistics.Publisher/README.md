# Publisher Functions

Azure Functions project responsible for a number of publishing-related tasks:

- **Release version publishing** - publishes approved release versions. A release version can either be scheduled for publication on a specific date or published immediately upon approval,
with two publisher flows handling these scenarios respectively. Includes copying of release version files from private to public storage.
- **Methodology version publishing** - publishes methodology versions alongside their associated release versions as
  part of the release publishing flows. Includes copying of methodology version files from private to public storage.
- **Methodology file publishing** - Copies methodology version files from private to public storage when a methodology version is approved for immediate
  public access, independently of a release version being published.
- **Taxonomy cache refresh** - refreshes the cached publication and methodology 'tree' data (content structure) used by the public site Methodologies, Data Catalogue, and Table Tool pages.

## Release version approval in the Admin and the `NotifyChange` function

When a release version is approved in Admin, a message is placed on a queue, triggering the `NotifyChange` function.

`NotifyChange` is the entry point into release version publishing and is responsible for routing to either the immediate or scheduled publishing flows.

At a high level, it:

1. Acquires a per-release lease to prevent concurrent processing of the same release version approval events.
2. Marks any previously scheduled publishing attempt for that release version as superseded.
3. Validates that the release version is in a valid state to publish (i.e. that it hasn't already been scheduled or started publishing).
4. Creates a new `ReleaseStatus` table entity in the Publisher's Azure Table storage to record details of the publishing attempt.
5. Routes to the correct flow:
    - **Immediate publication**: The status is created in an overall `Started` state, and it queues the 'Files' step to start immediately.
    - **Scheduled publication**: The status is created in an overall `Scheduled` state which the cron-triggered scheduled flow will pick up later.

## Immediate release version publishing flow

If `NotifyChange` is triggered by an approval set for immediate publication, it creates a release status entity in an overall
`Started` state and queues a message to trigger the 'Files' step function `PublishReleaseFiles` immediately.

The `PublishReleaseFiles` function copies release version files from private to public storage.
It also copies the methodology version files of any methodology versions being published alongside the release version.

It then performs the 'Publishing' step, delegating to `PublishingCompletionService` to complete all remaining publishing tasks.
One of the tasks involves updating the release version to make it publicly accessible.

When complete, the release status entity is updated with an overall `Complete` state.

## Scheduled release version publishing flow

If `NotifyChange` is triggered by an approval set for scheduled publication, it creates a release status entity in an overall `Scheduled` state.

Publishing of scheduled release relies on two functions which are triggered by cron schedules.

- **`StageScheduledReleases`** - Prepares release versions for publishing by triggering file copying.
- **`PublishScheduledReleaseVersions`** - Completes publishing of scheduled release versions.

## Forcing immediate publishing of scheduled release versions for testing

As scheduled publishing depends on two functions triggered by cron schedules, it can make testing frustrating if you have to wait for the next scheduled run.

To assist manual and automated testing, two additional HTTP-triggered functions have been developed to allow **scheduled** release versions to be published immediately.

By default, both functions target only release versions scheduled for publication on the current date.
Passing an array of specific release version IDs overrides this behaviour, causing only the specified versions to be targeted regardless of their scheduled publishing date.

All HTTP examples below target a local development environment.
Replace `http://localhost:7072` with the target environment's Publisher Functions base URL.

### StageScheduledReleaseVersionsImmediately

Prepare all release versions scheduled for publishing today:

```http request
POST http://localhost:7072/api/StageScheduledReleaseVersionsImmediately
Accept: application/json
```

Prepare specific release versions scheduled for publishing (ignores scheduled date):

```http request
POST http://localhost:7072/api/StageScheduledReleaseVersionsImmediately
Content-Type: application/json

{
  "releaseVersionIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

Response (release version IDs that are now prepared for publishing):

```json
{
  "releaseVersionIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

### PublishScheduledReleaseVersionsNow

Once preparation is complete, call this endpoint to finalise publishing and make the release versions live.

> **Note:** Allow sufficient time for the staging process to complete before calling this endpoint.

Publish all prepared release versions:

```
POST http://localhost:7072/api/PublishScheduledReleaseVersionsNow
Accept: application/json
```

Publish specific prepared release versions:

```
POST http://localhost:7072/api/PublishScheduledReleaseVersionsNow
Content-Type: application/json

{
  "releaseVersionIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

Response (release version IDs that have been published):

```json
{
  "releaseVersionIds": ["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]
}
```

### Triggering Functions via cURL

Here are some examples of using cURL to make the requests:

```bash
curl http://localhost:7072/api/StageScheduledReleaseVersionsImmediately -X POST -H 'Accept: application/json'
```

```bash
curl http://localhost:7072/api/StageScheduledReleaseVersionsImmediately -X POST -H 'Content-Type: application/json' -d '{"releaseVersionIds":["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]}'
```

```bash
curl http://localhost:7072/api/PublishScheduledReleaseVersionsNow -X POST -H 'Accept: application/json'
```

```bash
curl http://localhost:7072/api/PublishScheduledReleaseVersionsNow -X POST -H 'Content-Type: application/json' -d '{"releaseVersionIds":["6f6d5510-9f16-40ce-bc39-0dd88aa5e6ea", "c540405b-dc86-42fe-9420-3c3f007a48ef"]}'
```
