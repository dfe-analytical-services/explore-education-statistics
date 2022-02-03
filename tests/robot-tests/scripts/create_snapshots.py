import os
import json
import argparse
import requests
from bs4 import BeautifulSoup


def _gets_parsed_html_from_page(url):
    requests.sessions.HTTPAdapter(
        pool_connections=50,
        pool_maxsize=50,
        max_retries=3
    )
    session = requests.Session()
    response = session.get(url, stream=True)
    assert response.status_code == 200, f"Requests response wasn\'t 200!\nResponse: {response}"
    return BeautifulSoup(response.text, "html.parser")


def create_find_statistics_snapshot(public_url) -> str:
    find_stats_url = f"{public_url.rstrip('/')}/find-statistics"
    parsed_html = _gets_parsed_html_from_page(find_stats_url)

    themes_accordion = parsed_html.find(id="themes")
    if themes_accordion is None:
        return []

    theme_sections = themes_accordion.select('[data-testid="accordionSection"]') or []

    result = []
    for theme_index, theme_html in enumerate(theme_sections):
        theme = {
            'theme_heading': theme_html.select_one(f'#themes-{theme_index + 1}-heading').string,
            'topics': [],
        }

        topics = theme_html.select('[id^="topic-details-"]') or []
        for topic_html in topics:
            topic = {
                'topic_heading': topic_html.select_one('[id^="topic-heading-"]').string,
                'publication_types': [],
            }

            publication_types = topic_html.select('[id^="publication-type-heading-"]') or []
            for publication_type_html in publication_types:
                publication_type_key = publication_type_html['id'].replace('publication-type-heading-', '')
                publication_type = {
                    'publication_type_heading': publication_type_html.string,
                    'publications': [],
                }

                publications = topic_html.select(
                    f'ul[data-testid="publications-list-{publication_type_key}"] > li') or []
                for publication_html in publications:
                    publication = {
                        'publication_heading': publication_html.select_one('[id^="publication-heading-"]').string,
                    }

                    publication_type['publications'].append(publication)

                topic['publication_types'].append(publication_type)

            theme['topics'].append(topic)

        result.append(theme)

    return json.dumps(result, sort_keys=True, indent=2)


def create_table_tool_snapshot(public_url) -> str:
    table_tool_url = f"{public_url.rstrip('/')}/data-tables"
    parsed_html = _gets_parsed_html_from_page(table_tool_url)

    themes = parsed_html.select('[id^="theme-details-"]') or []

    result = []
    for theme_html in themes:
        theme = {
            'theme_heading': theme_html.select_one('[id^="theme-heading-"]').string,
            'topics': [],
        }

        topics = theme_html.select('[id^="topic-details-"]') or []
        for topic_html in topics:
            topic = {
                'topic_heading': topic_html.select_one('[id^="topic-heading-"]').string,
                'publications': [],
            }

            publications = topic_html.select('label') or []
            for publication_label in publications:
                topic['publications'].append(publication_label.string)

            theme['topics'].append(topic)

        result.append(theme)

    return json.dumps(result, sort_keys=True, indent=2)


def create_data_catalogue_snapshot(public_url) -> str:
    data_catalogue_url = f"{public_url.rstrip('/')}/data-catalogue"
    parsed_html = _gets_parsed_html_from_page(data_catalogue_url)

    themes = parsed_html.select('[id^="theme-details-"]') or []

    result = []
    for theme_html in themes:
        theme = {
            'theme_heading': theme_html.select_one('[id^="theme-heading-"]').string,
            'topics': [],
        }

        topics = theme_html.select('[id^="topic-details-"]') or []
        for topic_html in topics:
            topic = {
                'topic_heading': topic_html.select_one('[id^="topic-heading-"]').string,
                'publications': [],
            }

            publications = topic_html.select('label') or []
            for publication_label in publications:
                topic['publications'].append(publication_label.string)

            theme['topics'].append(topic)

        result.append(theme)

    return json.dumps(result, sort_keys=True, indent=2)


def create_all_methodologies_snapshot(public_url) -> str:
    all_methodologies_url = f"{public_url.rstrip('/')}/methodology"
    parsed_html = _gets_parsed_html_from_page(all_methodologies_url)

    methodologies_accordion = parsed_html.find(id="themes")
    if methodologies_accordion is None:
        return []

    theme_sections = methodologies_accordion.select('[data-testid="accordionSection"]') or []

    result = []

    for theme_index, theme_html in enumerate(theme_sections):
        theme = {
            'theme_heading': theme_html.select_one(f'#themes-{theme_index + 1}-heading').string,
            'topics': []
        }
        topics = theme_html.select('[id^="topic-details-"]') or []

        for topic_html in topics:
            topic = {
                'topic_heading': topic_html.select_one('[id^="topic-heading-"]').string,
                'methodologies': []
            }

            methodologies = topic_html.select('[id^="methodology-heading-"]') or []

            for methodology_heading in methodologies:
                topic['methodologies'].append(methodology_heading.string)

            theme['topics'].append(topic)

        result.append(theme)

    return json.dumps(result, sort_keys=True, indent=2)


def _write_to_file(file_name, snapshot):
    snapshots_path = 'tests/snapshots'
    if not os.path.exists(snapshots_path):
        os.makedirs(snapshots_path)

    path_to_file = os.path.join(os.getcwd(), snapshots_path, file_name)
    with open(path_to_file, 'w') as file:
        file.write(snapshot)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(prog=f"python {os.path.basename(__file__)}",
                                     description="To create snapshots of specific public frontend pages")
    parser.add_argument(dest="public_url",
                        default="https://explore-education-statistics.service.gov.uk",
                        nargs='?',
                        help="URL of public frontend you wish to create snapshots for")
    args = parser.parse_args()

    assert os.path.basename(os.getcwd()) == 'robot-tests', 'Must run from the robot-tests directory!'

    find_stats_snapshot = create_find_statistics_snapshot(args.public_url)
    _write_to_file('find_stats_snapshot.json', find_stats_snapshot)

    table_tool_snapshot = create_table_tool_snapshot(args.public_url)
    _write_to_file('table_tool_snapshot.json', table_tool_snapshot)

    data_catalogue_snapshot = create_data_catalogue_snapshot(args.public_url)
    _write_to_file('data_catalogue_snapshot.json', data_catalogue_snapshot)

    all_methodologies_snapshot = create_all_methodologies_snapshot(args.public_url)
    _write_to_file('all_methodologies_snapshot.json', all_methodologies_snapshot)
