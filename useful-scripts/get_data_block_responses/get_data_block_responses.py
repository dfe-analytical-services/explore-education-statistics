import requests
import json
import csv
import time
import os

"""
To generate datablocks.csv, use this SQL query:

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
"""

DATA_API_URL = 'https://data.explore-education-statistics.service.gov.uk/api'

datablocks = []

with open('datablocks.csv', 'r') as input:
    csv_reader = csv.reader(input, delimiter=',')
    for row in csv_reader:
        datablocks.append(row)

if not os.path.exists('results/fails'):
    os.makedirs('results/fails')

for datablock in datablocks:
    if datablock[0] == 'ContentBlockId':
        continue

    guid = datablock[0]
    releaseId = datablock[1]
    subjectId = datablock[2]
    query = datablock[3]

    if os.path.exists(f'results/block_{guid}') or os.path.exists(f'results/fails/block_{guid}'):
        continue

    url = f'{DATA_API_URL}/tablebuilder/release/{releaseId}'
    headers = {
        'Content-Type': 'application/json'
    }
    resp = requests.post(url=url,
                         headers=headers,
                         data=query
                         )

    file_path = 'results'
    if resp.status_code != 200:
        print(
            f'response status wasn\'t 200 for block {guid}\n'
            f'subject {subjectId}\n'
            f'{resp.text}\n'
        )
        file_path = 'results/fails'

    try:
        with open(f'{file_path}/block_{guid}', 'w') as file:
            file.write(
                f'block: {guid}\n'
                f'release: {releaseId}\n'
                f'subject: {subjectId}\n'
                f'query:\n'
                f'{query}\n'
                f'response status: {resp.status_code}\n'
                f'response:\n{resp.text}'
            )
    except:
        print(f'file.write failed with block {guid}\n'
              f'subject {subjectId}\n{resp.text}')

    time.sleep(1)
