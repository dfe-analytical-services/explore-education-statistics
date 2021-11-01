import requests
from bs4 import BeautifulSoup

requests.sessions.HTTPAdapter(
        pool_connections=50,
        pool_maxsize=50,
        max_retries=3
    )
session = requests.Session()

def user_gets_parsed_html_from_page(url):
    response = session.get(url, stream=True)
    return BeautifulSoup(response.text, "html.parser")


def user_gets_page_accordion_section(parsed_page, heading):
    sections = parsed_page.select('[data-testid="accordionSection"]')
    if not sections:
        raise AssertionError(f'No accordion sections found on page')

    for section in sections:
        heading_tag = section.select_one('.govuk-accordion__section-button')
        if not heading_tag:
            raise AssertionError('No heading for accordion section found!')
        if heading_tag.string == heading:
            return section
    raise AssertionError(f'Could not find accordion section with heading "{heading}"')


def user_gets_accordion_heading(accordion):
    heading = accordion.select_one('.govuk-accordion__section-button')
    if not heading:
        raise AssertionError('Accordion section heading wasn\'t found')
    return heading


def user_checks_accordion_heading_is_not_tag_type(accordion_heading, tag):
    if accordion_heading.name == tag:
        raise AssertionError(f'Accordion section heading shouldn\'t be tag type {tag} (with no javascript)')


def user_checks_accordion_heading_does_not_have_attribute_aria_expanded(accordion_heading):
    if 'aria-expanded' in accordion_heading.attrs:
        raise AssertionError('Accordion header contains "aria-expanded" attribute')


def user_gets_accordion_section_details(accordion, details_heading):
    content = accordion.select_one('.govuk-accordion__section-content')
    if not content:
        raise AssertionError('Accordion section content wasn\'t found!')

    all_details = accordion.select('details')
    if not all_details:
        raise AssertionError(f'No details nodes found in accordion')

    for details in all_details:
        heading = details.select_one(f'[data-testid="Expand Details Section {details_heading}"]')
        if heading:
            return details
    raise AssertionError(f'No details found in accordion with heading "{details_heading}"')


def user_checks_details_summary_does_not_have_attribute_aria_expanded(details):
    summary = details.select_one("summary")
    if not summary:
        raise AssertionError('No summary node found!')
    if 'aria-expanded' in summary.attrs:
        raise AssertionError('Details summary contains "aria-expanded" attribute')


def user_checks_details_contains_content(details, text):
    content = details.select_one('.govuk-details__text')
    if not content:
        raise AssertionError('Details on page should contain content')
    if text not in str(content):
        raise AssertionError(f'Could not find text "{text}" in details dropdown')