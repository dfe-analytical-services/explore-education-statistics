import argparse
import csv
import os
import time
from datetime import datetime

import certifi
import requests
from bs4 import BeautifulSoup

"""
This is a script for measuring the response time of permalink requests.
It uses the public frontend request `GET {public_base_url}/data-tables/permalink/{permalink_id}`.
It reads a CSV file named 'permalinks.csv' and outputs a CSV file `responses_{env}_{datetime}/responses.csv`.

Usage: `pipenv run python fetch_permalinks.py [-h] [--env {local,dev,test,preprod,prod}] [--save-content | --no-save-content] [--snapshot | --no-snapshot] [--sleep [SLEEP]] [--timeout [TIMEOUT]]`

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

3. Run the script, choosing whether to save the html content of the permalink requests and whether to use the new permalink snapshot feature.

4. Inspect the console log for errors and view the result CSV file `responses_{env}_{datetime}/responses.csv`.

You can compare two output directories together to see the differences between the html content of the permalink responses by running the following command:
`diff --brief --exclude=responses.csv dir1 dir2`

Note that if you are using Windows, you will need to install a tool like Git Bash to provide the `diff` command, or alternatively try a tool like WinMerge.
"""


class PermalinkFetcher:
    PUBLIC_URLS = {
        "local": "http://localhost:3000",
        "dev": "https://dev.explore-education-statistics.service.gov.uk",
        "test": "https://test.explore-education-statistics.service.gov.uk",
        "preprod": "https://pre-production.explore-education-statistics.service.gov.uk",
        "prod": "https://explore-education-statistics.service.gov.uk",
    }

    def __init__(self, env: str, save_content: bool, snapshot: bool, sleep: float, timeout: float):
        self.env = (env,)
        self.public_url = PermalinkFetcher.PUBLIC_URLS[env]
        self.save_content = save_content
        self.snapshot = snapshot
        self.session = requests.Session()
        self.sleep = sleep
        self.timeout = timeout
        self.http_headers = {"Accept-Encoding": "gzip, deflate, br"}
        self.results_dir = f"responses_{env}_{datetime.now().strftime('%Y%m%d_%H%M%S')}"

    def _get_permalink(self, permalink_id: str) -> tuple[int, bool, float, int, str]:
        url = f"{self.public_url}/data-tables/permalink/{permalink_id}"
        if self.snapshot:
            url += "?newPermalinks=true"
        response = self.session.get(
            url,
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

        # parse the response content using BeautifulSoup
        soup = BeautifulSoup(response.text, "lxml")

        # catch cases where the response is successful but the table is not rendered
        # and the error message "There was a problem rendering the table." appears instead
        table_error = False
        if response.status_code == 200 and soup.find("div", {"data-testid": "table-error"}):
            table_error = True

        # save the table html if the response is successful and the table is rendered
        if self.save_content and response.status_code == 200 and not table_error:
            self._save_table(soup, permalink_id)

        return response.status_code, table_error, response_time, content_length, content_length_formatted

    def _save_table(self, soup: BeautifulSoup, permalink_id: str) -> None:
        # find the data table figure element
        data_table = soup.find("figure", class_=lambda value: value and value.startswith("FixedMultiHeaderDataTable"))

        if data_table is None:
            print(f"Table not found for permalink Id {permalink_id}.")
        else:
            # get the parent of the figure element
            # as well as the the table itself, this contains the table caption, footnotes and the source
            data_table_parent = data_table.parent

            # assert that the parent element is a div tag
            assert data_table_parent.name == "div", "Parent element is not a div tag"

            # we want to make the table html comparable with the same html generated by an older version of EES pre EES-3778.
            # to make the comparison easier, make the following transformations to remove differences seen in every table:

            # (1) remove the borderBottom style class from td and th tags.
            # this class was removed by EES-3778 as the border is added by other table styles
            self._remove_style_class(data_table_parent, ["td", "th"], "MultiHeaderTable_borderBottom")

            # (2) remove empty class attributes from td tags.
            # empty class attributes still exist in th tags which is raised in issue EES-4350
            self._remove_empty_class_attributes(data_table_parent, ["td"])

            # (3) sort the remaining attributes alphabetically
            self._sort_tag_attributes(data_table_parent)

            html = str(data_table_parent)

            # (4) remove the visually hidden span tag from the footnotes heading.
            # this has multiple issues raised in EES-4349
            html = html.replace('Footnotes<span class="govuk-visually-hidden"> for undefined</span>', "Footnotes")

            self._write_content(html, permalink_id)

    def _remove_style_class(self, parent: BeautifulSoup, tag_names: list[str], ignored_class_prefix: str) -> None:
        for tag in parent.find_all(
            tag_names, class_=lambda value: value and any(c.startswith(ignored_class_prefix) for c in value.split())
        ):
            tag["class"] = " ".join(c for c in tag.get("class", []) if not c.startswith(ignored_class_prefix))

    def _remove_empty_class_attributes(self, parent: BeautifulSoup, tag_names: list[str]) -> None:
        for tag in parent.find_all(tag_names, class_=True):
            if tag.get("class") == "" or tag.get("class") == []:
                del tag["class"]

    def _sort_tag_attributes(self, parent: BeautifulSoup) -> None:
        for tag in parent.find_all():
            sorted_attrs = sorted(tag.attrs.items())
            tag.attrs = dict(sorted_attrs)

    def _write_content(self, content: str, permalink_id: str) -> None:
        with open(os.path.join(self.results_dir, f"{permalink_id}.html"), "w", encoding="utf-8") as output_file:
            output_file.write(content)

    def _get_permalinks(self, permalinks: list[list[str]]) -> None:
        with open(os.path.join(self.results_dir, "responses.csv"), "w", newline="") as output_file:
            output_writer = csv.writer(output_file)

            output_writer.writerow(
                [
                    "permalink_id",
                    "connection_error",
                    "timeout",
                    "exception",
                    "http_status_code",
                    "table_error",
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
                table_error: bool | None = None
                response_time: float | None = None
                content_length: int | None = None
                content_length_formatted: str | None = None

                try:
                    (
                        status_code,
                        table_error,
                        response_time,
                        content_length,
                        content_length_formatted,
                    ) = self._get_permalink(permalink_id)
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
                        table_error if table_error is True else "",
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
        if not os.path.exists(self.results_dir):
            os.makedirs(self.results_dir)

        with open("permalinks.csv", "r") as permalinks_csv_file:
            csv_reader = csv.reader(permalinks_csv_file)

            # Skip header row
            next(csv_reader)

            permalinks: list[list[str]] = list(csv_reader)

        self._get_permalinks(permalinks)


if __name__ == "__main__":
    ap = argparse.ArgumentParser(
        prog=f"pipenv run python {os.path.basename(__file__)}",
        description="Make permalink requests and record response details. Optionally can save the content of the permalink tables to files in the output directory so that they can be compared",
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
        "--save-content",
        dest="save_content",
        default=False,
        help="Save content of permalink tables to files in the output directory so that they can be compared",
        action=argparse.BooleanOptionalAction,
    )

    ap.add_argument(
        "--snapshot",
        dest="snapshot",
        default=False,
        help="Request the permalink page using the snapshot feature flag",
        action=argparse.BooleanOptionalAction,
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

    permalink_fetcher = PermalinkFetcher(
        env=args.env, save_content=args.save_content, snapshot=args.snapshot, sleep=args.sleep, timeout=args.timeout
    )
    permalink_fetcher.main()
