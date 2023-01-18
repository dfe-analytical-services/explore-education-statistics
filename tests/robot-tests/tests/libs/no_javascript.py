import os

import requests
from bs4 import BeautifulSoup

requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
session = requests.Session()
httpBasicAuth = requests.auth.HTTPBasicAuth


def user_gets_parsed_html_from_page(url):
    response = session.get(
        url, stream=False, auth=httpBasicAuth(os.getenv("PUBLIC_AUTH_USER"), os.getenv("PUBLIC_AUTH_PASSWORD"))
    )
    return BeautifulSoup(response.text, "html.parser")


def user_gets_publications_list(parsed_page):
    list = parsed_page.select_one('[data-testid="publicationsList"]')
    if not list:
        raise AssertionError("No publications list found on page")
    return list


def user_checks_list_contains_publication(list, publication):
    for publication_link in list.select(".govuk-link"):
        if publication_link.string == publication:
            return list
    raise AssertionError(f'Could not find publication "{publication}"')
