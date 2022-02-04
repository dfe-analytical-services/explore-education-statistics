import requests
import json
import csv
import time
import os
import datetime
import argparse

"""
To generate datablocks.csv, use this SQL query against the Content DB:

SELECT ContentBlock.Id AS ContentBlockId, Releases.Id AS ReleaseId, JSON_VALUE([DataBlock_Query], '$.SubjectId') AS SubjectId, ContentBlock.DataBlock_Query AS Query
  FROM ContentBlock
  JOIN ReleaseContentBlocks ON ContentBlock.Id = ReleaseContentBlocks.ContentBlockId
  JOIN Releases ON ReleaseContentBlocks.ReleaseId = Releases.Id
  Where ContentBlock.Type = 'DataBlock'
  AND Releases.Published IS NOT NULL
  AND SoftDeleted = 0
  AND (ContentSectionId IS NOT NULL
	   OR (DataBlock_HighlightName IS NOT NULL
	       AND DataBlock_HighlightName != ''));

And then save the results as a CSV in MS SQL Server Management Studio.
Place it in the same directory as this script.

Find blocks that took over 10 seconds to respond:
grep -r "time for response: [0-9][0-9][0-9]*" * | awk '{split($0,a,":"); print a[1];}' | zip -@ test.zip

Compare two result directories for differences, but ignoring response time (and any responses that are both Not Found responses, as they contain unique traceIds):
diff -I"^time for response:.*" -I "Not Found" -r results_dev1/ results_dev2/
"""

parser = argparse.ArgumentParser(prog="python get_data_block_responses.py",
                                 description="Used to get and time data block responses from "
                                             "an environment")
parser.add_argument("-e", "--env",
                    dest="env",
                    default="dev",
                    choices=["local", "dev", "test", "preprod", "prod"],
                    help="the environment to run again")
parser.add_argument("--stage",
                    dest="stage",
                    default="table",
                    choices=["table", "filters", "time_periods"],
                    help="the stage of the table tool to wish to get the response for")
parser.add_argument("-f", "--file",
                    dest="datablocks_csv",
                    default="datablocks.csv",
                    help="CSV of data blocks (see comment in this script)")
parser.add_argument("-s", "-sleep",
                    dest="sleep_duration",
                    default=1,
                    help="duration to sleep between requests",
                    type=int)
args = parser.parse_args()

data_api_urls = {
    'local': 'http://localhost:5000/api',
    'dev': 'https://data.dev.explore-education-statistics.service.gov.uk/api',
    'test': 'https://data.test.explore-education-statistics.service.gov.uk/api',
    'preprod': 'https://data.pre-production.explore-education-statistics.service.gov.uk/api',
    'prod': 'https://data.explore-education-statistics.service.gov.uk/api',
}

data_api_url = data_api_urls[args.env]

date = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
results_dir = f'results_{args.env}_{args.stage}_{date}'

datablocks = []

if not os.path.exists(results_dir):
    os.makedirs(results_dir)

with open(f'{results_dir}/console_output', 'w') as file:
    file.write('Console output: \n\n')

output_file = open(f'{results_dir}/console_output', 'a')


def print_to_console(text):
    output_file.write(text + '\n')
    print(text)


with open(args.datablocks_csv, 'r') as input:
    csv_reader = csv.reader(input, delimiter=',')
    for row in csv_reader:
        datablocks.append(row)

start_time = time.perf_counter()
for datablock in datablocks:
    if datablock[0] == 'ContentBlockId':
        continue

    global queryDict
    global url

    guid = datablock[0]
    releaseId = datablock[1]
    subjectId = datablock[2]
    queryDict = json.loads(datablock[3])

    if args.stage == "table":
        url = f'{data_api_url}/tablebuilder/release/{releaseId}'

    if args.stage == "filters":
        queryDict.pop("Filters")
        queryDict.pop("Indicators")
        url = f'{data_api_url}/meta/subject'

    if args.stage == "time_periods":
        queryDict.pop("Filters")
        queryDict.pop("Indicators")
        queryDict.pop("TimePeriod")
        url = f'{data_api_url}/meta/subject'

    query = json.dumps(queryDict, sort_keys=True, indent=2)

    assert url is not None
    assert query is not None

    if os.path.exists(f'{results_dir}/block_{guid}'):
        continue

    headers = {
        'Content-Type': 'application/json'
    }

    block_time_start = time.perf_counter()
    try:
        resp = requests.post(url=url,
                             headers=headers,
                             data=query,
                             timeout=120)
    except Exception as e:
        print_to_console(f'request exception with block {guid} subject {subjectId}\nException: {e}')
        jsonResponse = {'error': f'request exception thrown, {e}'}
    block_time_end = time.perf_counter()

    if resp.status_code != 200:
        print_to_console(
            f'Response status wasn\'t 200 for block {guid} '
            f'subject {subjectId}'
            f'{resp.text}'
        )

    try:
        jsonResponse = json.loads(resp.text)
    except Exception as e:
        print_to_console(f'json.loads(resp.text) failed with block {guid} '
                         f'subject {subjectId}\n'
                         f'Exception: {e}')
        jsonResponse = {'error': f'Failed to process response text, Exception: {e}'}

    try:
        with open(f'{results_dir}/block_{guid}', 'w') as file:
            file.write(
                f'block: {guid}\n'
                f'release: {releaseId}\n'
                f'subject: {subjectId}\n'
                f'query:\n'
                f'{query}\n'
                f'response status: {resp.status_code}\n'
                f'time for response: {block_time_end - block_time_start}\n'
                f'response:\n{json.dumps(jsonResponse, sort_keys=True, indent=2)}'
            )
        print_to_console(f'Successfully processed block {guid} for subject {subjectId}!')
    except BaseException:
        print_to_console(f'file.write failed with block {guid} '
                         f'subject {subjectId}\n{resp.text}')

    time.sleep(args.sleep_duration)

end_time = time.perf_counter()
print_to_console(f'Total time: {end_time - start_time}')
print_to_console(f'Sleep time: {args.sleep_duration * len(datablocks)}')
print_to_console(f'Total minus sleep time: {(end_time - start_time) - (args.sleep_duration * len(datablocks))}')
