import csv
import os
from logging import warning
from enum import Enum

assert os.getenv('PUBLIC_URL') is not None
public_url = os.getenv('PUBLIC_URL')


class DataBlockType(Enum):
    CONTENT_BLOCK = 1,
    FAST_TRACK = 2,
    SECONDARY_STATS = 3,
    KEY_STATS = 4


class DataBlockRow:
    def __init__(
            self,
            content_block_id,
            release_id,
            release_slug,
            publication_id,
            publication_slug,
            content_section_id,
            content_section_heading,
            content_section_type,
            highlight_name,
            subject_id,
            chart_title,
            chart_type,
            table_config):

        self.publication_id = publication_id.lower()
        self.chart_type = chart_type
        self.chart_title = chart_title
        self.subject_id = subject_id.lower()
        self.highlight_name = highlight_name
        self.content_section_type = content_section_type
        self.content_section_id = None if content_section_id is None else content_section_id.lower()
        self.publication_slug = publication_slug
        self.release_slug = release_slug
        self.release_id = release_id.lower()
        self.content_block_id = content_block_id.lower()
        self.content_section_heading = content_section_heading
        self.table_config = table_config is not None

        if self.content_section_type is None:
            self.type = DataBlockType.FAST_TRACK
        elif self.content_section_type == 'KeyStatisticsSecondary':
            self.type = DataBlockType.SECONDARY_STATS
        elif self.content_section_type == 'Generic':
            self.type = DataBlockType.CONTENT_BLOCK
        elif self.content_section_type == 'KeyStatistics':
            self.type = DataBlockType.KEY_STATS
        else:
            raise Error(f'Unhandled Content Section Type {self.content_section_type}')

        if self.type == DataBlockType.FAST_TRACK:
            self.content_url = f'{public_url}/data-tables/fast-track/{self.content_block_id}'
        else:
            self.content_url = f'{public_url}/find-statistics/{self.publication_slug}/{self.release_slug}'


content_blocks = []


def read_cell(cell_value):
    if cell_value == 'NULL':
        return None
    return cell_value


with open('local-datablocks.csv', 'r', encoding='utf-8-sig') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    for row in csv_reader:
        content_blocks.append(DataBlockRow(
            read_cell(row[0]),
            read_cell(row[1]),
            read_cell(row[2]),
            read_cell(row[3]),
            read_cell(row[4]),
            read_cell(row[5]),
            read_cell(row[6]),
            read_cell(row[7]),
            read_cell(row[8]),
            read_cell(row[9]),
            read_cell(row[10]),
            read_cell(row[11]),
            read_cell(row[12])))


def get_content_blocks():
    return content_blocks
