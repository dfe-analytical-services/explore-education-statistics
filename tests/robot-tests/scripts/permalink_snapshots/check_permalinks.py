import csv
import os
import time

import requests
from bs4 import BeautifulSoup
from requests import Response

"""
Script for checking the health of permalinks & whether they can be rendered.
Some permalinks on production don't load i.e. https://explore-education-statistics.service.gov.uk/data-tables/permalink/aed871a3-61e5-4f2a-8a2b-cceeca680980
This will cause issues when migrating to permalink snapshots (see https://dfedigital.atlassian.net/browse/EES-3784)

Instructions:

To generate the csv file of permalinks run the following query against the content DB:

```
SELECT *
from [dbo].[Permalinks]
```

place these permalinks in a csv file in the same directory as '~/permalink-snapshots'
with the filename 'prod-permalinks.csv'

create a csv file with the name 'invalid-permalinks.csv' in the same directory as '~/permalink-snapshots'
with the following headers:

```
permalink_id,subject_name,publication_name,status_code,has_table,created_date,out_of_date,error_message,warning_message,response_time
```

create a csv file with the name 'checked-permalinks.csv' in the same directory as '~/permalink-snapshots'
with the following headers:

```
permalink_id, response_time
```

"""


class PermalinkType:
    id = str
    publicationTitle = str
    dataSetTitle = str
    releaseId = str
    subjectId = str
    created = str


requests.sessions.HTTPAdapter(pool_connections=100, pool_maxsize=100, max_retries=3)


class PermalinkChecker:
    def __init__(self):
        # change 'expected_row_length' to the number of rows in the 'prod-permalinks.csv' file
        self.expected_row_length = 23310
        self.backoff = 1.75
        self.request_timeout = 15
        self.session = requests.Session()
        self.prod_permalinks_csv = os.path.join(os.getcwd() + "/scripts/permalink_snapshots", "prod-permalinks.csv")
        self.invalid_permalinks_csv = os.path.join(
            os.getcwd() + "/scripts/permalink_snapshots", "invalid-permalinks.csv"
        )
        self.checked_permalinks_csv = os.path.join(
            os.getcwd() + "/scripts/permalink_snapshots", "checked-permalinks.csv"
        )
        self.public_url = "https://explore-education-statistics.service.gov.uk"
        self.permalink_base_url = f"{self.public_url}/data-tables/permalink/"

        assert os.path.basename(os.getcwd()) == "robot-tests", "Must run from the robot-tests directory"

    def _get_permalinks_from_csv(self) -> list[PermalinkType]:
        with open(self.prod_permalinks_csv, "r") as csv_file:
            reader = csv.reader(csv_file, delimiter=",")
            # skip header
            next(reader)
            permalinks = []
            for row in reader:
                permalinks.append(
                    {
                        "id": row[0],
                        "publicationTitle": row[1],
                        "dataSetTitle": row[2],
                        "releaseId": row[3],
                        "subjectId": row[4],
                        "created": row[5],
                    }
                )
            csv_file.close()

        if not len(permalinks) == self.expected_row_length:
            raise Exception(f"Expected {self.expected_row_length} rows, but got {len(permalinks)}")

        print(f"Loaded {len(permalinks)} permalinks from csv file")

        return permalinks

    def _build_permalink_urls(self):
        permalinks = self._get_permalinks_from_csv()

        return [f"{self.permalink_base_url}{permalink['id']}" for permalink in permalinks]

    def _write_status_to_csv(self, response: Response, permalink_id: str):
        if not isinstance(response, Response):
            # if the response is not a Response object (we set the response variable to a string initially),
            # then we assume that the maximum timeout has been reached and we couldn't load the permalink
            permalink_error = "Timed out"
            subject_name = " "
            publication_name = " "
            has_table = False
            permalink_warning_message = " "
            response_time = " "
            all_permalinks = self._get_permalinks_from_csv()

            for permalink in all_permalinks:
                if permalink["id"] == permalink_id:
                    created_date = permalink["created"]

            print(f"Request timed out for permalink {permalink_id}. Writing to invalid permalinks file")

            with open(self.invalid_permalinks_csv, "a", newline="") as csvfile:
                writer = csv.writer(csvfile, delimiter=",")
                writer.writerow(
                    [
                        permalink_id,
                        subject_name if subject_name else " ",
                        publication_name if publication_name else " ",
                        response.status_code if isinstance(response, Response) else " ",
                        has_table,
                        created_date,
                        permalink_error,
                        permalink_warning_message,
                        response_time if isinstance(response, Response) else " ",
                    ]
                )
                csvfile.close()

        else:
            # if the response is a Response object, then we know that the permalink loaded successfully
            # and can access the response object's attributes

            permalink_warning_testid = "permalink-warning"
            permalink_table_error_testid = "table-error"
            permalink_error = ""
            permalink_out_of_date = False
            has_table = False
            has_permalink_error = False
            all_permalinks = self._get_permalinks_from_csv()
            response_time = response.elapsed.total_seconds()

            for permalink in all_permalinks:
                if permalink["id"] == permalink_id:
                    created_date = permalink["created"]

            soup = BeautifulSoup(response.text, "html.parser")

            page_title = soup.find("h1")

            if page_title.text == "Sorry, there's a problem with the service":
                permalink_error_message = "Sorry, there's a problem with the service"
                has_permalink_error = True
                has_permalink_warning_message = False

                print(f"500 error for permalink {permalink_id}. Writing to invalid permalinks file")

            elif page_title.text == "Page not found":
                permalink_error_message = "Page not found"
                has_permalink_error = True
                has_permalink_warning_message = False
                print(f"404 error for permalink {permalink_id}. Writing to invalid permalinks file")

            try:
                subject_name = page_title.text.split("from")[0].strip()
                publication_name = page_title.text.split("from")[1].strip()

            except IndexError:
                subject_name = " "
                publication_name = " "

            # permalink has a warning
            if soup.find("div", {"data-testid": permalink_warning_testid}):
                permalink_warning_message = (
                    soup.find("div", {"data-testid": permalink_warning_testid}).find("strong").text.strip()
                )

                has_permalink_warning_message = True
                print(f"Permalink {permalink_id} has a warning")
                if (
                    permalink_warning_message
                    == "WarningWARNING - The data used in this table may now be out-of-date as a new release has been published since its creation"
                ):
                    permalink_out_of_date = True

            if soup.find("table"):
                has_table = True

            # permalink has an error or rendering issues
            if soup.find("div", {"data-testid": permalink_table_error_testid}):
                has_permalink_error = True
                permalink_error_message = (
                    soup.find("div", {"data-testid": permalink_table_error_testid}).find("strong").text.strip()
                )
                print("Permalink has an error. Writing to invalid permalinks file")

            with open(self.invalid_permalinks_csv, "a", newline="") as csvfile:
                writer = csv.writer(csvfile, delimiter=",")

                if has_table and not permalink_error:
                    print(f"Not writing permalink {permalink_id} to csv as it has no errors")

                elif has_table and not page_title:
                    # should never happen as all permalinks should have a page title
                    # could be due to a massive data table in the permalink so add it to the csv
                    # so we can check manually
                    print(f"Writing permalink {permalink_id} to csv as it has no page title")
                    writer.writerow(
                        [
                            permalink_id,
                            subject_name if subject_name else " ",
                            publication_name if publication_name else " ",
                            response.status_code,
                            has_table,
                            created_date,
                            permalink_out_of_date if permalink_out_of_date else False,
                            permalink_error_message if has_permalink_error else " ",
                            permalink_warning_message if has_permalink_warning_message else " ",
                            response_time,
                        ]
                    )
                else:
                    # permalink has an error or rendering issues
                    print(f"Writing permalink {permalink_id} to csv as it has an error")
                    writer.writerow(
                        [
                            permalink_id,
                            subject_name if subject_name else " ",
                            publication_name if publication_name else " ",
                            response.status_code,
                            has_table,
                            created_date,
                            permalink_out_of_date if permalink_out_of_date else False,
                            permalink_error_message if has_permalink_error else " ",
                            permalink_warning_message if has_permalink_warning_message else " ",
                            response_time,
                        ],
                    )
                csvfile.close()

    def _write_permalink_id_to_checked_permalinks_file(self, permalink_id: str, response_time: float):
        with open(self.checked_permalinks_csv, "a", newline="") as csvfile:
            writer = csv.writer(csvfile, delimiter=",")
            writer.writerow([permalink_id, response_time])
            csvfile.close()

    def check_permalinks(self):
        permalink_urls = self._build_permalink_urls()

        start = time.time()

        for url in permalink_urls:
            permalink_id = url.split("/")[-1]
            if permalink_id not in open(self.checked_permalinks_csv).read():
                print("-----------------------------------")
                print(f"Checking permalink {permalink_id}")
                print("-----------------------------------")

                response = ""
                response_time = ""

                try:
                    response = self.session.get(url, timeout=self.request_timeout)
                    response_time = response.elapsed.total_seconds()
                    print(f"Took {response_time} seconds to load permalink")
                    self._write_status_to_csv(response, permalink_id)
                    self._write_permalink_id_to_checked_permalinks_file(permalink_id, response_time)
                except requests.exceptions.Timeout:
                    self._write_status_to_csv(response, permalink_id)
                    self._write_permalink_id_to_checked_permalinks_file(permalink_id, response_time)

                time.sleep(self.backoff)
            else:
                print(f"Skipping permalink {permalink_id} as it has already been checked")

        end = time.time()

        print(f"Checked {len(permalink_urls)} permalinks in {round((end - start) / 3600, 2)} hours")


if __name__ == "__main__":
    permalink_checker = PermalinkChecker()
    permalink_checker.check_permalinks()
