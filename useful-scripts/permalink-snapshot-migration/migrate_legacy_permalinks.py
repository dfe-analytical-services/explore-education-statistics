import argparse
import csv
import os
import time
from datetime import datetime

import certifi
import requests

"""
This is the script for migrating legacy permalinks to permalink snapshots.
It uses a Data API migration endpoint `PUT /api/permalink/{permalinkId}/snapshot`.
It reads a CSV file named 'permalinks.csv' and outputs a CSV file named 'migration_results_{env}_{datetime}.csv'.

Usage: `pipenv run python migrate_legacy_permalinks.py [-h] [--env {local,dev,test,preprod,prod}] [--sleep [SLEEP]] [--timeout [TIMEOUT]]`

Instructions:

1. List all the legacy permalinks which do not have a snapshot by running a query against the Content database:

```
SELECT LOWER(Id) AS permalink_id
FROM content.dbo.Permalinks
WHERE Legacy = 1
AND LegacyHasSnapshot IS NULL
ORDER BY LegacyContentLength DESC
```

2. Place the results of the database query with column headers in a new file.
Save it in the same directory as this script with filename 'permalinks.csv'.

Example of 'permalinks.csv' input file:

```
permalink_id
a227b04a-42af-4139-28e5-08db35ad5bb4
a57e9ae9-b442-4005-caec-08db3b6d5a38
```

3. Run the script.
4. Inspect the console log for errors and view the result CSV file 'migration_results.csv'.
5. Refresh the database query and update the input file before re-running.
"""


class MigrateLegacyPermalinks:
    DATA_API_URLS = {
        "local": "http://localhost:5000/api",
        "dev": "https://data.dev.explore-education-statistics.service.gov.uk/api",
        "test": "https://data.test.explore-education-statistics.service.gov.uk/api",
        "preprod": "https://data.pre-production.explore-education-statistics.service.gov.uk/api",
        "prod": "https://data.explore-education-statistics.service.gov.uk/api",
    }

    def __init__(self, env: str, sleep: float, timeout: float):
        self.env = env
        self.data_api_url = MigrateLegacyPermalinks.DATA_API_URLS[env]
        self.session = requests.Session()
        self.timeout = timeout
        self.sleep = sleep
        self.http_headers = {}

    def _migrate_permalink(self, permalink_id: str) -> tuple[int, float]:
        start_time = time.monotonic()
        response = self.session.put(
            f"{self.data_api_url}/permalink/{permalink_id}/snapshot",
            headers=self.http_headers,
            timeout=self.timeout,
            verify=certifi.where(),
        )
        end_time = time.monotonic()
        response_time = end_time - start_time
        print(
            f"Request to migrate permalink Id {permalink_id} "
            f"returned status {response.status_code} in {response_time:.2f} seconds"
        )
        return response.status_code, response_time

    def _migrate_permalinks(self, permalinks: list[list[str]]) -> None:
        output_filename = f"migration_results_{self.env}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.csv"
        with open(output_filename, "w", newline="") as output_file:
            output_writer = csv.writer(output_file)

            output_writer.writerow(
                [
                    "permalink_id",
                    "connection_error",
                    "timeout",
                    "exception",
                    "http_status_code",
                    "response_time",
                ]
            )
            output_file.flush()

            for (permalink_id,) in permalinks:
                connection_error: bool | None = None
                timeout: bool | None = None
                exception: bool | None = None
                status_code: int | None = None
                response_time: float | None = None

                try:
                    status_code, response_time = self._migrate_permalink(permalink_id)
                except requests.exceptions.ConnectionError:
                    connection_error = True
                    print(f"Request to migrate permalink Id {permalink_id} failed to connect")
                except requests.exceptions.Timeout:
                    timeout = True
                    print(f"Request to migrate permalink Id {permalink_id} timed out after {self.timeout} seconds")
                except requests.exceptions.RequestException as e:
                    exception = True
                    print(f"Request to migrate permalink Id {permalink_id} failed with error: {str(e)}")

                # write a result row to the output csv file
                output_writer.writerow(
                    [
                        permalink_id,
                        connection_error,
                        timeout,
                        exception,
                        status_code,
                        "" if response_time is None else f"{response_time:.2f}",
                    ]
                )
                output_file.flush()

                # sleep for a specified amount of time since generating the snapshot is resource intensive
                time.sleep(self.sleep)

    def main(self):
        with open("permalinks.csv", "r") as permalinks_csv_file:
            csv_reader = csv.reader(permalinks_csv_file)

            # Skip header row
            next(csv_reader)

            permalinks: list[list[str]] = list(csv_reader)

        self._migrate_permalinks(permalinks)


if __name__ == "__main__":
    ap = argparse.ArgumentParser(
        prog=f"pipenv run python {os.path.basename(__file__)}",
        description="To migrate legacy permalinks to permalink snapshots",
    )

    ap.add_argument(
        "--env",
        dest="env",
        default="local",
        choices=["local", "dev", "test", "preprod", "prod"],
        help="The environment to run against",
        type=str,
        required=False,
    )

    ap.add_argument(
        "--sleep",
        dest="sleep",
        default=5.0,
        nargs="?",
        help="Delay between migration request executions in number of seconds",
        type=float,
        required=False,
    )

    ap.add_argument(
        "--timeout",
        dest="timeout",
        default=240.0,
        nargs="?",
        help="Timout for the migration request in number of seconds",
        type=float,
        required=False,
    )

    args = ap.parse_args()

    migrator = MigrateLegacyPermalinks(env=args.env, sleep=args.sleep, timeout=args.timeout)
    migrator.main()
