# Searchable Docs Function App

Azure Functions project which maintains the searchable documents that Azure AI Search indexes for the public site:

- **Release documents** - one per publication, for its latest published release. Used by Find Statistics.
- **Data set documents** - one per data file in the latest published version of each release.

Each document is stored as a blob in the Search storage account. An Azure AI Search indexer for each document type
reads the blobs into its index.

Most functions are triggered by a queue. Event handlers receive events raised by the Admin and Publisher, such as a
release version being published, and queue commands. Command handlers act on those commands by getting documents
from the Content API, uploading or removing blobs, and running the indexers.

This page covers the HTTP endpoints, which are for checking and repairing the documents by hand.

## HTTP endpoints

When running locally the base URL is `http://localhost:7075`. In Azure, access to the Function App is restricted by
IP address.

| Method | Route                            | Purpose                                                          |
|--------|----------------------------------|------------------------------------------------------------------|
| GET    | `/api/HealthCheck`               | Confirms the Function App is running.                            |
| GET    | `/api/FullHealthCheck`           | Checks the Content API, blob storage and Azure AI Search too.    |
| GET    | `/api/ReleaseDocumentCheck`      | Compares the release document blobs with those expected.         |
| GET    | `/api/DataSetDocumentCheck`      | Compares the data set document blobs with those expected.        |
| POST   | `/api/FullReleaseDocumentsReset` | Removes all release documents and queues them for recreation.    |
| POST   | `/api/FullDataSetDocumentsReset` | Removes all data set documents and queues them for recreation.   |

## Checking the documents

`ReleaseDocumentCheck` and `DataSetDocumentCheck` ask the Content API which documents should exist and compare that
with the blobs in storage. They change nothing.

The report lists the blobs which are missing and the blobs which shouldn't be there. Both lists are empty when the
blobs are correct. The checks compare blob names only: they don't detect a blob whose content is out of date, or a
difference between the blobs and the search index.

## Resetting the documents

Use a reset when a check reports missing or extra blobs, or when documents are known to be out of date, for example
after a change to the content of the documents has been deployed.

Both resets work the same way:

1. Get the list of publications from the Content API. If this fails nothing is removed.
2. Remove every blob in the container.
3. Queue one refresh command per publication.

Each refresh command creates the documents of its publication again and causes the indexer to run. The index is
incomplete from the moment the blobs are removed until the refresh commands have completed and the indexer has run,
so searches return fewer results for a short time.

`FullDataSetDocumentsReset` responds with the number of blobs it removed and the number of publications it queued a
refresh for:

```json
{
  "removedBlobCount": 1234,
  "publicationCount": 321
}
```

To confirm a reset has finished:

1. Wait for the refresh queue to empty: `refresh-publication-data-set-documents-queue` for data set documents, or
   `refresh-release-document-queue` for release documents.
2. Check that the matching poison queue (the queue name followed by `-poison`) is empty. A refresh command which
   keeps failing ends up there, and its publication's documents are left missing.
3. Call `DataSetDocumentCheck` or `ReleaseDocumentCheck` and confirm that no blobs are missing or extra.

## Resetting an indexer

A document reset rewrites every blob, so the indexer picks all of them up without needing to be reset itself.

If the index still differs from the blobs after a document reset, reset and run the indexer in the Azure portal as a
fallback: open the Azure AI Search service, choose the indexer under 'Indexers', then 'Reset' followed by 'Run'.
This makes the indexer read every blob again on its next run. The indexer names are held in the
`AzureSearch__ReleaseIndexerName` and `AzureSearch__DataSetIndexerName` app settings.
