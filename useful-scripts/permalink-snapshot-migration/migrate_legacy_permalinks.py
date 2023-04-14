import argparse
import csv
import os
import time

import requests

"""
This is the script for migrating legacy permalinks to permalink snapshots.
It uses a Data API migration endpoint `PUT /api/permalink/{permalinkId}/snapshot`.
It reads a CSV file named 'permalinks.csv' and outputs a CSV file named 'migration_results.csv'.

Usage: `pipenv run python migrate_legacy_permalinks.py`

Instructions:

1. List all the legacy permalinks by running a query against the Content database to get all the legacy permalinks:

```
SELECT LOWER(Id) AS permalink_id,
       IIF(LegacyHasSnapshot = 1, 'True', 'False') AS migrated
FROM content.dbo.Permalinks
WHERE Legacy = 1
ORDER BY Created
```

2. Place the results of the database query with column headers in a new file.
Save it in the same directory as this script with filename 'permalinks.csv'.

Example of 'permalinks.csv' input file:

```
permalink_id,migrated
a227b04a-42af-4139-28e5-08db35ad5bb4,False
a57e9ae9-b442-4005-caec-08db3b6d5a38,False
```

3. Run the script.
4. Inspect the console log for errors and view the result CSV file 'migration_results.csv'.
5. Refresh the database query and update the input file before re-running.

"""


class MigrateLegacyPermalinks:
    def __init__(self, data_api_url: str):
        self.data_api_url = data_api_url
        self.http_headers = {}

    def _migrate_permalink(self, permalink_id, timeout):
        try:
            start_time = time.monotonic()
            response = requests.put(
                f"{self.data_api_url}/permalink/{permalink_id}/snapshot",
                headers=self.http_headers,
                timeout=timeout,
                verify=False,
            )
            end_time = time.monotonic()
            response_time = end_time - start_time
            print(
                f"Request for permalink Id {permalink_id} returned status {response.status_code} in {response_time:.2f} seconds"
            )
            return response.status_code, response_time
        except requests.exceptions.RequestException as e:
            print(f"Request for permalink Id {permalink_id} failed with error: {str(e)}")
            return None, None

    def _migrate_permalinks(self, permalinks, timeout, sleep):
        with open("migration_results.csv", "w", newline="") as output_file:
            output_writer = csv.writer(output_file)

            output_writer.writerow(["permalink_id", "http_status_code", "response_time"])
            output_file.flush()

            for permalink_id, migrated in permalinks:
                # if the permalink does not already have a snapshot, make a request to the Data API to migrate it
                if migrated == "False":
                    status_code, response_time = self._migrate_permalink(permalink_id, timeout)

                    # write the status code and response time to an output csv file
                    output_writer.writerow([permalink_id, status_code, f"{response_time:.2f}"])
                    output_file.flush()

                # sleep for the specified amount of time since generating the snapshot is resource intensive
                time.sleep(sleep)

    def main(self):
        with open("permalinks.csv", "r") as permalinks_csv_file:
            csv_reader = csv.reader(permalinks_csv_file)
            permalinks = list(csv_reader)

        self._migrate_permalinks(permalinks, timeout=240, sleep=5)


if __name__ == "__main__":
    ap = argparse.ArgumentParser(
        prog=f"python {os.path.basename(__file__)}", description="To migrate legacy permalinks to permalink snapshots"
    )

    ap.add_argument(
        "--data-api-url",
        dest="data_api_url",
        default="http://localhost:5000/api",
        nargs="?",
        help="URL of the Data API e.g. http://localhost:5000/api",
        type=str,
        required=False,
    )

    args = ap.parse_args()

    migrator = MigrateLegacyPermalinks(data_api_url=args.data_api_url)
    migrator.main()
