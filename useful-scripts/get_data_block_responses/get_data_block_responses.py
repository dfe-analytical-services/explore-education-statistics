import argparse
import csv
import datetime
import json
import os
import time

import requests

"""
To generate datablocks.csv, use this SQL query against the Content DB:

SELECT ContentBlock.Id                              AS ContentBlockId,
       Releases.Id                                  AS ReleaseId,
       JSON_VALUE([DataBlock_Query], '$.SubjectId') AS SubjectId,
       ContentBlock.DataBlock_Query                 AS Query
FROM ContentBlock
JOIN ReleaseContentBlocks ON ContentBlock.Id = ReleaseContentBlocks.ContentBlockId
JOIN Releases ON ReleaseContentBlocks.ReleaseId = Releases.Id
LEFT JOIN KeyStatisticsDataBlock ON ContentBlock.Id = KeyStatisticsDataBlock.DataBlockId
WHERE ContentBlock.Type = 'DataBlock'
  AND Releases.Published IS NOT NULL
  AND Releases.SoftDeleted = 0
  AND (
    -- Include DataBlocks that are linked to Content Sections
    ContentSectionId IS NOT NULL
    -- Include DataBlocks that are Key Statistics
    OR KeyStatisticsDataBlock.DataBlockId IS NOT NULL
    -- Include DataBlocks that are Featured Tables
    OR (DataBlock_HighlightName IS NOT NULL
        AND DataBlock_HighlightName <> ''))
  -- Include only DataBlocks that are from the latest published Release
  AND NOT EXISTS(
    SELECT 1
    FROM Releases PublicationReleases
    WHERE PublicationReleases.PublicationId = Releases.PublicationId
      AND PublicationReleases.Published IS NOT NULL
      AND PublicationReleases.SoftDeleted = 0
      AND PublicationReleases.Id <> Releases.Id
      AND PublicationReleases.PreviousVersionId = Releases.Id
  );

And then save the results as a CSV in MS SQL Server Management Studio.
Place it in the same directory as this script.

Find blocks that took over 10 seconds to respond:
grep -r "time for response: [0-9][0-9][0-9]*" * | awk '{split($0,a,":"); print a[1];}' | zip -@ test.zip

Compare two result directories for differences, but ignoring response time (and any responses that are both Not Found
responses, as they contain unique traceIds):
diff -I"^time for response:.*" -I "Not Found" -r results_dev1/responses results_dev2/responses
"""

parser = argparse.ArgumentParser(
    prog="python get_data_block_responses.py",
    description="Used to get and time data block responses from " "an environment",
)
parser.add_argument(
    "-e",
    "--env",
    dest="env",
    default="dev",
    choices=["local", "dev", "test", "preprod", "prod"],
    help="the environment to run again",
)
parser.add_argument(
    "--stage",
    dest="stage",
    default="table",
    choices=["table", "filters", "time_periods"],
    help="the stage of the table tool to wish to get the response for",
)
parser.add_argument(
    "-f",
    "--file",
    dest="datablocks_csv",
    default="datablocks.csv",
    help="CSV of data blocks (see comment in this script)",
)
parser.add_argument(
    "-s", "-sleep", dest="sleep_duration", default=1, help="duration to sleep between requests", type=int
)
args = parser.parse_args()

data_api_urls = {
    "local": "http://localhost:5000/api",
    "dev": "https://data.dev.explore-education-statistics.service.gov.uk/api",
    "test": "https://data.test.explore-education-statistics.service.gov.uk/api",
    "preprod": "https://data.pre-production.explore-education-statistics.service.gov.uk/api",
    "prod": "https://data.explore-education-statistics.service.gov.uk/api",
}

data_api_url = data_api_urls[args.env]

date = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
results_dir = f"results_{args.env}_{args.stage}_{date}"
requests_dir = f"{results_dir}/requests"
responses_dir = f"{results_dir}/responses"

if not os.path.exists(requests_dir):
    os.makedirs(requests_dir)

if not os.path.exists(responses_dir):
    os.makedirs(responses_dir)

with open(f"{results_dir}/console_output", "w") as output_file:
    output_file.write("Console output:\n\n")

output_file = open(f"{results_dir}/console_output", "a")


def print_to_console(text):
    output_file.write(text + "\n")
    print(text)


def write_block_to_file(
    url, block_guid, release_guid, subject_guid, request_dict, status_code, response_time, response_dict
):
    try:
        with open(f"{requests_dir}/block_{block_guid}_request", "w") as block_request_file:
            block_request_file.write(
                f"block: {block_guid}\n"
                f"release: {release_guid}\n"
                f"subject: {subject_guid}\n"
                f"url: {url}\n"
                f"request:\n{json.dumps(request_dict, sort_keys=True, indent=2)}"
            )

        with open(f"{responses_dir}/block_{block_guid}_response", "w") as block_response_file:
            block_response_file.write(
                f"block: {block_guid}\n"
                f"release: {release_guid}\n"
                f"subject: {subject_guid}\n"
                f"response status: {status_code}\n"
                f"time for response: {response_time}\n"
                f"response:\n{json.dumps(response_dict, sort_keys=True, indent=2)}"
            )
    except Exception as exception:
        print_to_console(
            f"block_file.write failed with block {block_guid} subject {subject_guid}\n"
            f"Response: {json.dumps(response_dict)}\n Exception: {exception}"
        )


processed = 0
processed_successfully = 0
datablocks = []
with open(args.datablocks_csv, "r") as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=",")
    for row in csv_reader:
        if row[0] == "ContentBlockId":
            continue
        datablocks.append(row)

start_time = time.perf_counter()
for datablock in datablocks:
    processed += 1
    time.sleep(args.sleep_duration)

    block_id = datablock[0]
    release_id = datablock[1]
    subject_id = datablock[2]
    query_dict = json.loads(datablock[3])

    if args.stage == "table":
        url = f"{data_api_url}/tablebuilder/release/{release_id}"

    if args.stage == "filters":
        url = f"{data_api_url}/meta/subject"
        query_dict.pop("Filters")
        query_dict.pop("Indicators")

    if args.stage == "time_periods":
        url = f"{data_api_url}/meta/subject"
        query_dict.pop("Filters")
        query_dict.pop("Indicators")
        query_dict.pop("TimePeriod")

    def write_this_block(status_code, response_time, response_dict):
        write_block_to_file(
            url=url,
            block_guid=block_id,
            release_guid=release_id,
            subject_guid=subject_id,
            request_dict=query_dict,
            status_code=status_code,
            response_time=response_time,
            response_dict=response_dict,
        )

    block_time_start = time.perf_counter()
    try:
        resp = requests.post(
            url=url, headers={"Content-Type": "application/json"}, data=json.dumps(query_dict), timeout=60
        )
    except requests.Timeout as e:
        print_to_console(f"request timeout with block {block_id} subject {subject_id}, {e}")
        write_this_block(status_code=-1, response_time=-1, response_dict={"error": f"request timeout, {e}"})
        continue
    except Exception as e:
        print_to_console(f"request exception with block {block_id} subject {subject_id}, {e}")
        write_this_block(status_code=-1, response_time=-1, response_dict={"error": f"request exception thrown, {e}"})
        continue

    block_time_end = time.perf_counter()

    if resp.text is None or resp.text == "":
        json_response = ""
    else:
        json_response = json.loads(resp.text)

    if resp.status_code == 200:
        print_to_console(f"Successfully processed block {block_id} for subject {subject_id}!")
        processed_successfully += 1
    else:
        print_to_console(f"{resp.status_code} response received for block {block_id} subject {subject_id}")

    write_this_block(
        status_code=resp.status_code, response_time=block_time_end - block_time_start, response_dict=json_response
    )

end_time = time.perf_counter()
processing_time = round(end_time - start_time)
sleep_time = args.sleep_duration * len(datablocks)
processing_time_minus_sleep_time = round(processing_time - sleep_time)

print_to_console(f"Total processed: {processed}")
print_to_console(f"Total successes: {processed_successfully}")
print_to_console(f"Total failures: {processed - processed_successfully}")
print_to_console(f"Total time: {processing_time} seconds")
print_to_console(f"Sleep time: {sleep_time} seconds")
print_to_console(f"Total minus sleep time: {processing_time_minus_sleep_time} seconds")
print_to_console(
    f"Average processing time per block: {round(processing_time_minus_sleep_time / len(datablocks), 2)} seconds"
)
