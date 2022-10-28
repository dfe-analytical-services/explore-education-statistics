import csv
from enum import Enum

releases_by_url = {}


class DataBlockType(Enum):
    CONTENT_BLOCK = 1
    FAST_TRACK = 2
    SECONDARY_STATS = 3
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
        content_section_position,
        min_content_section_position,
        content_block_position,
        min_content_block_position,
        highlight_name,
        subject_id,
        chart_title,
        chart_type,
    ):

        self.publication_id = publication_id.lower()
        self.chart_type = chart_type
        self.chart_title = chart_title
        self.subject_id = subject_id.lower()
        self.highlight_name = highlight_name
        self.content_section_type = content_section_type
        self.content_section_position = (
            int(content_section_position) + 1 - int(min_content_section_position)
            if content_section_position is not None
            else None
        )
        self.content_block_position = (
            int(content_block_position) + 1 - int(min_content_block_position)
            if min_content_block_position is not None
            else None
        )
        self.content_section_id = None if content_section_id is None else content_section_id.lower()
        self.publication_slug = publication_slug
        self.release_slug = release_slug
        self.release_id = release_id.lower()
        self.content_block_id = content_block_id.lower()
        self.content_section_heading = content_section_heading
        self.has_table_config = True
        self.has_chart_config = chart_type is not None

        if self.content_section_type is None:
            self.type = DataBlockType.FAST_TRACK
        elif self.content_section_type == "KeyStatisticsSecondary":
            self.type = DataBlockType.SECONDARY_STATS
        elif self.content_section_type == "Generic":
            self.type = DataBlockType.CONTENT_BLOCK
        elif self.content_section_type == "KeyStatistics":
            self.type = DataBlockType.KEY_STATS
        else:
            raise AttributeError(f"Unhandled Content Section Type {self.content_section_type}")

        if self.type == DataBlockType.FAST_TRACK:
            self.content_url = f"/data-tables/fast-track/{self.content_block_id}"
        else:
            self.content_url = f"/find-statistics/{self.publication_slug}/{self.release_slug}"


class Release:
    def __init__(
        self,
        publication_id,
        publication_slug,
        release_id,
        release_slug,
        key_stat_blocks,
        secondary_stat_blocks,
        content_section_blocks,
        fast_track_blocks,
        permalink_blocks,
    ):
        self.publication_id = publication_id
        self.publication_slug = publication_slug
        self.release_slug = release_slug
        self.release_id = release_id
        self.key_stat_blocks = key_stat_blocks
        self.secondary_stat_blocks = secondary_stat_blocks
        self.content_section_blocks = content_section_blocks
        self.fast_track_blocks = fast_track_blocks
        self.permalink_blocks = permalink_blocks
        self.url = f"/find-statistics/{self.publication_slug}/{self.release_slug}"
        self.has_key_stat_blocks = bool(key_stat_blocks)
        self.has_secondary_stat_blocks = bool(secondary_stat_blocks)
        self.has_content_section_blocks = bool(content_section_blocks)
        self.has_fast_track_blocks = bool(fast_track_blocks)
        self.has_permalinks = bool(permalink_blocks)


def generate_releases(data_blocks_csv_filepath):

    content_blocks = []

    def read_cell(cell_value):
        if cell_value == "NULL":
            return None
        return cell_value

    with open(data_blocks_csv_filepath, "r", encoding="utf-8-sig") as csv_file:
        csv_reader = csv.reader(csv_file, delimiter=",")
        next(csv_reader)
        for row in csv_reader:
            content_blocks.append(
                DataBlockRow(
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
                    read_cell(row[12]),
                    read_cell(row[13]),
                    read_cell(row[14]),
                    read_cell(row[15]),
                )
            )

    release_ids = sorted(set(map(lambda block: block.release_id, content_blocks)))

    def create_release(release_id):
        release_blocks = list(filter(lambda block: block.release_id == release_id, content_blocks))
        key_stats = list(filter(lambda block: block.type == DataBlockType.KEY_STATS, release_blocks))
        secondary_stats = list(filter(lambda block: block.type == DataBlockType.SECONDARY_STATS, release_blocks))
        content_section_blocks = list(filter(lambda block: block.type == DataBlockType.CONTENT_BLOCK, release_blocks))
        fast_tracks = list(filter(lambda block: block.type == DataBlockType.FAST_TRACK, release_blocks))
        permalinks = []
        return Release(
            release_blocks[0].publication_id,
            release_blocks[0].publication_slug,
            release_blocks[0].release_id,
            release_blocks[0].release_slug,
            key_stats,
            secondary_stats,
            content_section_blocks,
            fast_tracks,
            permalinks,
        )

    releases = map(create_release, release_ids)

    for release in releases:
        releases_by_url[release.url] = release


def get_release_by_url(url: str):
    return releases_by_url[url]


def get_releases_by_url():
    return releases_by_url
