import csv
import os
from logging import warning

assert os.getenv('PUBLIC_URL') is not None
public_url = os.getenv('PUBLIC_URL')

datablocks = []
with open('local-datablocks.csv', 'r') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    for row in csv_reader:
        if row[0] == "ContentBlockId":
            continue
        datablocks.append(row)


def get_datablock_urls():

    urls = []
    for datablock in datablocks:
        publication_slug = datablock[4]
        release_slug = datablock[2]
        urls.append(f'{public_url}/find-statistics/{publication_slug}/{release_slug}')

    warning(urls)
    return urls
