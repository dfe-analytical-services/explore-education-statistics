import csv
import os
from logging import warning

assert os.getenv('PUBLIC_URL') is not None
public_url = os.getenv('PUBLIC_URL')

content_blocks = []
fast_tracks = []

with open('local-datablocks.csv', 'r') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    for row in csv_reader:
        if row[0] == "ContentBlockId":
            continue
        if row[6] == 'NULL':
            fast_tracks.append(row)
        else:
            content_blocks.append(row)


def get_content_block_urls():

    urls = []
    for datablock in content_blocks:
        publication_slug = datablock[4]
        release_slug = datablock[2]
        urls.append(f'{public_url}/find-statistics/{publication_slug}/{release_slug}')

    warning(urls)
    return urls


def get_fast_track_urls():

    urls = []
    for datablock in fast_tracks:
        content_block_id = datablock[0]
        urls.append(f'{public_url}/data-tables/fast-track/{content_block_id}')

    warning(urls)
    return urls
