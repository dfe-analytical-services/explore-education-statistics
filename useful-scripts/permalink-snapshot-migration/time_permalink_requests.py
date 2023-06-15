import argparse
import csv
import os
import time

import certifi
import requests

"""
This is a script for measuring the response time of permalink requests.
It uses the public frontend request `GET {public_base_url}/data-tables/permalink/{permalink_id}.
It reads a CSV file named 'permalinks.csv' and outputs a CSV file named 'response_times.csv'.

Usage: `pipenv run python time_permalink_requests.py [-h] [--public-url [PUBLIC_URL]] [--sleep [SLEEP]] [--timeout [TIMEOUT]]`

Instructions:

1. List all the legacy permalinks by running a query against the Content database:

```
SELECT LOWER(Id) AS permalink_id
FROM content.dbo.Permalinks
WHERE Legacy = 1
ORDER BY Created
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
4. Inspect the console log for errors and view the result CSV file 'timing_results.csv'.
"""


class TimePermalinkRequests:
    def __init__(self, public_url: str, sleep: float, timeout: float):
        self.public_url = public_url
        self.session = requests.Session()
        self.timeout = timeout
        self.sleep = sleep
        self.http_headers = {"Accept-Encoding": "gzip, deflate, br"}

    def _time_permalink_request(self, permalink_id: str) -> tuple[int, float, int, str]:
        response = self.session.get(
            f"{self.public_url}/data-tables/permalink/{permalink_id}",
            headers=self.http_headers,
            timeout=self.timeout,
            verify=certifi.where(),
        )
        response_time = response.elapsed.total_seconds()
        content_length = len(response.content)
        content_length_formatted = self._format_content_length(content_length)
        print(
            f"Request for permalink Id {permalink_id} "
            f"returned status {response.status_code}, duration: {response_time:.2f} seconds, "
            f"length: {content_length_formatted}."
        )
        return response.status_code, response_time, content_length, content_length_formatted

    def _time_permalink_requests(self, permalinks: list[list[str]]) -> None:
        with open("response_times.csv", "w", newline="") as output_file:
            output_writer = csv.writer(output_file)

            output_writer.writerow(
                [
                    "permalink_id",
                    "connection_error",
                    "timeout",
                    "exception",
                    "http_status_code",
                    "response_time",
                    "content_length",
                    "content_length_formatted",
                ]
            )
            output_file.flush()

            for (permalink_id,) in permalinks:
                connection_error: bool | None = None
                timeout: bool | None = None
                exception: bool | None = None
                status_code: int | None = None
                response_time: float | None = None
                content_length: int | None = None
                content_length_formatted: str | None = None

                try:
                    status_code, response_time, content_length, content_length_formatted = self._time_permalink_request(
                        permalink_id
                    )
                except requests.exceptions.ConnectionError:
                    connection_error = True
                    print(f"Request for permalink Id {permalink_id} failed to connect")
                except requests.exceptions.Timeout:
                    timeout = True
                    print(f"Request for permalink Id {permalink_id} timed out after {self.timeout} seconds")
                except requests.exceptions.RequestException as e:
                    exception = True
                    print(f"Request for permalink Id {permalink_id} failed with error: {str(e)}")

                # write a result row to the output csv file
                output_writer.writerow(
                    [
                        permalink_id,
                        connection_error,
                        timeout,
                        exception,
                        status_code,
                        "" if response_time is None else f"{response_time:.2f}",
                        content_length,
                        content_length_formatted,
                    ]
                )
                output_file.flush()

                # sleep for a specified amount of time since requesting big permalinks is resource intensive
                time.sleep(self.sleep)

    def _format_content_length(self, length: int) -> str:
        if length < 1024:
            return f"{length} bytes"
        elif length < 1024 * 1024:
            return f"{length / 1024:.2f} Kb"
        else:
            return f"{length / (1024 * 1024):.2f} Mb"

    def main(self):
        with open("permalinks.csv", "r") as permalinks_csv_file:
            csv_reader = csv.reader(permalinks_csv_file)

            # Skip header row
            next(csv_reader)

            permalinks: list[list[str]] = list(csv_reader)

        self._time_permalink_requests(permalinks)


if __name__ == "__main__":
    ap = argparse.ArgumentParser(
        prog=f"pipenv run python {os.path.basename(__file__)}",
        description="Measure the response time of permalink requests",
    )

    ap.add_argument(
        "--public-url",
        dest="public_url",
        default="http://localhost:3000",
        nargs="?",
        help="URL of the Public site e.g. http://localhost:3000",
        type=str,
        required=False,
    )

    ap.add_argument(
        "--sleep",
        dest="sleep",
        default=5.0,
        nargs="?",
        help="Delay between request executions in number of seconds",
        type=float,
        required=False,
    )

    ap.add_argument(
        "--timeout",
        dest="timeout",
        default=240.0,
        nargs="?",
        help="Timout for the request in number of seconds",
        type=float,
        required=False,
    )

    args = ap.parse_args()

    time_permalink_requests = TimePermalinkRequests(public_url=args.public_url, sleep=args.sleep, timeout=args.timeout)
    time_permalink_requests.main()
