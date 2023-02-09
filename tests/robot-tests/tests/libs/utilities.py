import base64
import datetime
import json
import os
import re
import time
from typing import Union
from urllib.parse import urlparse

import pytz
import utilities_init
import visual
from robot.libraries.BuiltIn import BuiltIn
from selenium.webdriver.common.by import By
from selenium.webdriver.remote.webelement import WebElement
from SeleniumLibrary.keywords.waiting import WaitingKeywords
from SeleniumLibrary.utils import is_noney
from tests.libs.logger import get_logger

sl = BuiltIn().get_library_instance("SeleniumLibrary")
element_finder = sl._element_finder
waiting = WaitingKeywords(sl)
logger = get_logger(__name__)

# Should only initialise some parts once e.g. registration
# of custom locators onto the framework's ElementFinder
if not utilities_init.initialised:

    def _normalize_parent_locator(parent_locator: object) -> Union[str, WebElement]:
        if not isinstance(parent_locator, str) and not isinstance(parent_locator, WebElement):
            return "css:body"

        return parent_locator

    def _find_by_label(parent_locator: object, criteria: str, tag: str, constraints: dict) -> list:
        parent_locator = _normalize_parent_locator(parent_locator)

        labels = get_child_elements(parent_locator, f'xpath:.//label[text()="{criteria}"]')

        if len(labels) == 0:
            return []

        for_id = labels[0].get_attribute("for")
        return get_child_elements(parent_locator, f"id:{for_id}")

    def _find_by_testid(parent_locator: object, criteria: str, tag: str, constraints: dict) -> list:
        parent_locator = _normalize_parent_locator(parent_locator)

        return get_child_elements(parent_locator, f'css:[data-testid="{criteria}"]')

    # Register locator strategies

    element_finder.register("label", _find_by_label, persist=True)
    element_finder.register("testid", _find_by_testid, persist=True)

    utilities_init.initialised = True


def enable_basic_auth_headers():
    # Setup basic auth headers for public frontend
    public_auth_user = os.getenv("PUBLIC_AUTH_USER")
    public_auth_password = os.getenv("PUBLIC_AUTH_PASSWORD")

    if public_auth_user and public_auth_password:
        token = base64.b64encode(f"{public_auth_user}:{public_auth_password}".encode())

        sl.driver.execute_cdp_cmd("Network.enable", {})
        sl.driver.execute_cdp_cmd(
            "Network.setExtraHTTPHeaders", {"headers": {"Authorization": f"Basic {token.decode()}"}}
        )


def disable_basic_auth_headers():
    # Must be disabled to visit admin frontend
    sl.driver.execute_cdp_cmd("Network.disable", {})


def raise_assertion_error(err_msg):
    sl.failure_occurred()
    raise AssertionError(err_msg)


def user_waits_until_parent_contains_element(
    parent_locator: object, child_locator: str, timeout: int = None, error: str = None, count: int = None
):
    try:
        child_locator = _normalise_child_locator(child_locator)

        def parent_contains_matching_element() -> bool:
            parent_el = _get_parent_webelement_from_locator(parent_locator, timeout, error)
            return element_finder.find(child_locator, required=False, parent=parent_el) is not None

        if is_noney(count):
            return waiting._wait_until(
                parent_contains_matching_element,
                "Parent '%s' did not contain '%s' in <TIMEOUT>." % (parent_locator, child_locator),
                timeout,
                error,
            )

        count = int(count)

        def parent_contains_matching_elements() -> bool:
            parent_el = _get_parent_webelement_from_locator(parent_locator, timeout, error)
            return len(sl.find_elements(child_locator, parent=parent_el)) == count

        waiting._wait_until(
            parent_contains_matching_elements,
            "Parent '%s' did not contain %s '%s' element(s) within <TIMEOUT>." % (parent_locator, count, child_locator),
            timeout,
            error,
        )
    except Exception as err:
        logger.warn(
            f"Error whilst executing utilities.py user_waits_until_parent_contains_element() "
            f"with parent {parent_locator} and child locator {child_locator} - {err}"
        )
        raise_assertion_error(err)


def user_waits_until_parent_does_not_contain_element(
    parent_locator: object, child_locator: str, timeout: int = None, error: str = None, count: int = None
):
    try:
        child_locator = _normalise_child_locator(child_locator)

        def parent_does_not_contain_matching_element() -> bool:
            parent_el = _get_parent_webelement_from_locator(parent_locator, timeout, error)
            return element_finder.find(child_locator, required=False, parent=parent_el) is None

        if is_noney(count):
            return waiting._wait_until(
                parent_does_not_contain_matching_element,
                "Parent '%s' should not have contained '%s' in <TIMEOUT>." % (parent_locator, child_locator),
                timeout,
                error,
            )

        count = int(count)

        def parent_does_not_contain_matching_elements() -> bool:
            parent_el = _get_parent_webelement_from_locator(parent_locator, timeout, error)
            return len(sl.find_elements(child_locator, parent=parent_el)) != count

        waiting._wait_until(
            parent_does_not_contain_matching_elements,
            "Parent '%s' should not have contained %s '%s' element(s) within <TIMEOUT>."
            % (parent_locator, count, child_locator),
            timeout,
            error,
        )
    except Exception as err:
        logger.warn(
            f"Error whilst executing utilities.py "
            f"user_waits_until_parent_does_not_contain_element() with parent {parent_locator} "
            f"and child locator {child_locator} - {err}"
        )
        raise_assertion_error(err)


def get_child_element(parent_locator: object, child_locator: str):
    try:
        children = get_child_elements(parent_locator, child_locator)

        if len(children) == 0:
            raise_assertion_error(
                f"Found no elements matching child locator {child_locator} under parent "
                f"locator {parent_locator} in utilities.py#get_child_element()"
            )

        if len(children) > 1:
            logger.warn(
                f"Found {len(children)} child elements matching child locator {child_locator} "
                f"under parent locator {parent_locator} in utilities.py#get_child_element() - "
                f"was expecting only one. Consider making the parent selector more specific. "
                f"Returning the first element found."
            )

        return children[0]
    except Exception as err:
        logger.warn(
            f"Error whilst executing utilities.py get_child_element() with parent {parent_locator} and child "
            f"locator {child_locator} - {err}"
        )
        raise_assertion_error(err)


def get_child_elements(parent_locator: object, child_locator: str):
    try:
        child_locator = _normalise_child_locator(child_locator)
        parent_el = _get_parent_webelement_from_locator(parent_locator)
        return element_finder.find_elements(child_locator, parent=parent_el)
    except Exception as err:
        logger.warn(f"Error whilst executing utilities.py get_child_elements() - {err}")
        raise_assertion_error(err)


def user_waits_for_page_to_finish_loading():
    # This is required because despite the DOM being loaded, and even a button being enabled,
    # React/NextJS hasn't finished processing the page, and so click are intermittently ignored.
    # I'm wrapping this sleep in a keyword such that if we find a way to check whether the JS
    # processing has finished in the future, we can change it here.
    time.sleep(0.2)


def user_sets_focus_to_element(selector):
    sl.wait_until_page_contains_element(selector)
    sl.set_focus_to_element(selector)


def set_to_local_storage(key: str, value: str):
    sl.execute_javascript(f"localStorage.setItem('{key}', '{value}');")


def set_cookie_from_json(cookie_json):
    cookie_dict = json.loads(cookie_json)
    del cookie_dict["domain"]

    sl.driver.add_cookie(cookie_dict)


def format_uk_to_local_datetime(uk_local_datetime: str, strf: str) -> str:
    if os.name == "nt":
        strf = strf.replace("%-", "%#")

    tz = pytz.timezone("Europe/London")

    return tz.localize(datetime.datetime.fromisoformat(uk_local_datetime)).astimezone().strftime(strf)


def get_current_datetime(strf: str, offset_days: int = 0) -> str:
    return format_datetime(datetime.datetime.now() + datetime.timedelta(days=offset_days), strf)


def format_datetime(datetime: datetime, strf: str) -> str:
    if os.name == "nt":
        strf = strf.replace("%-", "%#")

    return datetime.strftime(strf)


def user_should_be_at_top_of_page():
    (x, y) = sl.get_window_position()
    if y != 0:
        raise_assertion_error(f"Windows position Y is {y} not 0! User should be at the top of the page!")


def prompt_to_continue():
    logger.warn("Continue? (Y/n)")
    choice = input()
    if choice.lower().startswith("n"):
        raise_assertion_error("Tests stopped!")


def capture_large_screenshot_and_prompt_to_continue():
    visual.capture_large_screenshot()
    prompt_to_continue()


def capture_large_screenshot_and_html():
    visual.capture_large_screenshot()
    capture_html()


def capture_html():
    html = sl.get_source()
    current_time_millis = round(datetime.datetime.timestamp(datetime.datetime.now()) * 1000)
    html_file = open(f"test-results/captured-html-{current_time_millis}.html", "w", encoding="utf-8")
    html_file.write(html)
    html_file.close()
    logger.warn(f"Captured HTML of {sl.get_location()}      HTML saved to file://{os.path.realpath(html_file.name)}")


def user_gets_row_number_with_heading(heading: str, table_locator: str = "css:table"):
    elem = get_child_element(table_locator, f'xpath:.//tbody/tr/th[text()="{heading}"]/..')
    rows = get_child_elements(table_locator, "css:tbody tr")
    return rows.index(elem) + 1


def user_gets_row_with_group_and_indicator(group: str, indicator: str, table_selector: str = "css:table"):
    table_elem = sl.get_webelement(table_selector)
    elems = table_elem.find_elements(
        By.XPATH,
        f'.//tbody/tr/th[text()="{group}"]/../self::tr | .//tbody/tr/th[text()="{group}"]/../following-sibling::tr',
    )
    for elem in elems:
        try:
            elem.find_element(By.XPATH, f'.//th[text()="{indicator}"]/..')
            return elem
        except BaseException:
            continue
    raise_assertion_error(f'Indicator "{indicator}" not found!')


def user_checks_row_cell_contains_text(row_elem, cell_num, expected_text):
    try:
        elem = get_child_element(row_elem, f"xpath:.//td[{cell_num}]")
    except BaseException:
        raise_assertion_error(f'Couldn\'t find TD tag num "{cell_num}" for provided row element')

    if expected_text not in elem.text:
        raise_assertion_error(
            f'TD tag num "{cell_num}" for row element didn\'t contain text "{expected_text}".'
            f'Found text "{elem.text}"'
        )


def remove_substring_from_right_of_string(string, substring):
    if string.endswith(substring):
        return string[: -len(substring)]
    else:
        raise_assertion_error(f'String "{string}" doesn\'t end with substring "{substring}"')


def user_clicks_element_if_exists(selector):
    if element_finder.find(selector, required=False) is not None:
        sl.click_element(selector)


def user_is_on_admin_dashboard(admin_url: str) -> bool:
    current_url = sl.get_location()
    url_parts = urlparse(current_url)
    left_part = f"{url_parts.scheme}://{url_parts.netloc}{url_parts.path}"
    if left_part.endswith("/"):
        left_part = left_part[:-1]
    return left_part == admin_url or left_part == f"{admin_url}/dashboard"


def user_is_on_admin_dashboard_with_theme_and_topic_selected(admin_url: str, theme: str, topic: str) -> bool:
    if not user_is_on_admin_dashboard(admin_url):
        return False
    selected_theme = sl.get_selected_list_label("id:publicationsReleases-themeTopic-themeId")
    if selected_theme != theme:
        return False
    selected_topic = sl.get_selected_list_label("id:publicationsReleases-themeTopic-topicId")
    return selected_topic == topic


def user_navigates_to_admin_dashboard_if_needed(admin_url: str):
    disable_basic_auth_headers()
    if user_is_on_admin_dashboard(admin_url):
        return

    sl.go_to(admin_url)


def is_webelement(variable: object) -> bool:
    return isinstance(variable, WebElement)


def _normalise_child_locator(child_locator: str) -> str:
    if isinstance(child_locator, str):
        # the below substitution is necessary in order to correctly find the parent's descendants.  Without the
        # preceding dot, the double forward slash breaks out of the parent container and returns the xpath query
        # to the root of the DOM, leading to false positives or incorrectly found DOM elements.  The below
        # substitution covers both child selectors beginning with "xpath://" and "//", as the double forward
        # slashes without the "xpath:" prefix are inferred as being xpath expressions.
        return re.sub(r"^(xpath:)?//", "xpath:.//", child_locator)

    raise_assertion_error(f"Child locator was not a str - {child_locator}")


def _get_parent_webelement_from_locator(parent_locator: object, timeout: int = None, error: str = "") -> WebElement:
    if isinstance(parent_locator, str):
        sl.wait_until_page_contains_element(parent_locator, timeout=timeout, error=error)
        return sl.find_element(parent_locator)
    elif isinstance(parent_locator, WebElement):
        return parent_locator
    else:
        raise_assertion_error(f"Parent locator was neither a str or a WebElement - {parent_locator}")
