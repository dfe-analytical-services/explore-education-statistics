import json
import os
import re
import time
from typing import Union
from urllib.parse import urlparse, urlunparse

import utilities_init
from robot.libraries.BuiltIn import BuiltIn
from selenium.common.exceptions import NoSuchElementException, StaleElementReferenceException
from selenium.webdriver.common.by import By
from selenium.webdriver.remote.webelement import WebElement
from SeleniumLibrary.utils import is_noney
from tests.libs.logger import get_logger
from tests.libs.selenium_elements import element_finder, sl

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

    def _find_by_text(parent_locator: object, criteria: str, tag: str, constraints: dict) -> list:
        parent_locator = _normalize_parent_locator(parent_locator)

        return get_child_elements(parent_locator, f'xpath:.//*[contains(., "{criteria}")]')

    # Register locator strategies

    element_finder().register("label", _find_by_label, persist=True)
    element_finder().register("testid", _find_by_testid, persist=True)
    element_finder().register("text", _find_by_text, persist=True)

    utilities_init.initialised = True


def get_url_with_basic_auth(url: str):
    public_auth_user = os.getenv("PUBLIC_AUTH_USER")
    public_auth_password = os.getenv("PUBLIC_AUTH_PASSWORD")

    if public_auth_user and public_auth_password:
        basic_auth_credentials = f"{public_auth_user}:{public_auth_password}"
        if basic_auth_credentials in url:
            return url
        url_parts = url.split("://")
        if len(url_parts) == 2:
            return f"{url_parts[0]}://{basic_auth_credentials}@{url_parts[1]}"
        else:
            return f"{basic_auth_credentials}@{url_parts[0]}"
    else:
        return url


def raise_assertion_error(err_msg):
    sl().failure_occurred()
    raise AssertionError(err_msg)


def retry_or_fail_with_delay(func, retries=5, delay=1.0, *args, **kwargs):
    last_exception = None
    for attempt in range(retries):
        try:
            return func(*args, **kwargs)
        except Exception as e:
            last_exception = e
            logger.info(f"Attempt {attempt + 1}/{retries} failed with error: {e}. Retrying in {delay} seconds...")
            time.sleep(delay)
    # Raise the last exception if all retries failed
    raise last_exception


def wait_until_parent_contains_element(
    parent_locator_or_element: object,
    child_locator: str,
    timeout: int = None,
    error: str = None,
    count: int = None,
):
    retry_delay = 1
    default_timeout = BuiltIn().get_variable_value("${TIMEOUT}")
    retries = int(timeout / retry_delay if timeout is not None else int(default_timeout) / retry_delay)
    normalised_child_locator = _normalise_child_locator(child_locator)
    expected_count = None if is_noney(count) else count

    def check_correct_number_of_matching_child_elements_are_present():
        parent_el = _get_webelement_from_locator(parent_locator_or_element, timeout=0.1, error=error)
        child_elements = sl().find_elements(normalised_child_locator, parent=parent_el)

        if expected_count is None and len(child_elements) == 0:
            raise AssertionError(
                f"Expected at least one matching child element but found none "
                f"under parent {parent_locator_or_element} using child selector {normalised_child_locator}"
            )

        if expected_count is not None and len(child_elements) != expected_count:
            raise AssertionError(
                f"Expected {expected_count} child elements but found {len(child_elements)} "
                f"under parent {parent_locator_or_element} using child selector {normalised_child_locator}"
            )

    return do_with_retries(
        action=check_correct_number_of_matching_child_elements_are_present,
        action_description="wait_until_parent_contains_element",
        allowed_exception_types=AssertionError,
        retries=retries,
        retry_delay=retry_delay,
    )


def wait_until_parent_does_not_contain_element(
    parent_locator_or_element: object, child_locator: str, timeout: int = None, error: str = None
):
    retry_delay = 1
    default_timeout = BuiltIn().get_variable_value("${TIMEOUT}")
    retries = int(timeout / retry_delay if timeout is not None else int(default_timeout) / retry_delay)
    normalised_child_locator = _normalise_child_locator(child_locator)

    def check_child_elements_are_not_present():
        parent_el = _get_webelement_from_locator(parent_locator_or_element, timeout=0.1, error=error)
        child_elements = sl().find_elements(normalised_child_locator, parent=parent_el)

        if len(child_elements) > 0:
            raise AssertionError(
                f"Expected no matching child elements but found {len(child_elements)} "
                f"under parent {parent_locator_or_element} using child selector {normalised_child_locator}"
            )

    return do_with_retries(
        action=check_child_elements_are_not_present,
        action_description="user_waits_until_parent_does_not_contain_element",
        allowed_exception_types=AssertionError,
        retries=retries,
        retry_delay=retry_delay,
    )


def get_child_element(parent_locator: object, child_locator: str, retries: int = 5, delay: float = 1.0):
    def get_element():
        children = get_child_elements(parent_locator, child_locator)
        if not children:
            raise NoSuchElementException(
                f"No elements matching child locator '{child_locator}' under parent locator '{parent_locator}'"
            )
        if len(children) > 1:
            logger.warning(
                f"Multiple ({len(children)}) elements found for child locator '{child_locator}' under parent locator '{parent_locator}'. "
                f"Returning the first element. Consider refining the parent selector."
            )
        return children[0]

    return do_with_retries(get_element, "get_child_element", NoSuchElementException, retries, delay)


def get_child_elements(parent_locator: object, child_locator: str):
    try:
        child_locator = _normalise_child_locator(child_locator)
        parent_el = _get_webelement_from_locator(parent_locator)
        return element_finder().find_elements(child_locator, parent=parent_el)
    except Exception as err:
        logger.warning(f"Error whilst executing utilities.py get_child_elements() - {err}")
        raise_assertion_error(err)


def user_sets_focus_to_element(selector):
    sl().wait_until_page_contains_element(selector)
    sl().set_focus_to_element(selector)


def user_scrolls_element_to_center_of_view(locator_or_element: object):
    def scroll_to_element():
        element = _get_webelement_from_locator(locator_or_element)
        sl().driver.execute_script('arguments[0].scrollIntoView({behavior: "instant", block: "center"})', element)

    do_with_retries(
        scroll_to_element,
        "user_scrolls_element_to_center_of_view",
        StaleElementReferenceException,
        retries=3,
        retry_delay=2,
    )


def set_cookie_from_json(cookie_json):
    cookie_dict = json.loads(cookie_json)
    del cookie_dict["domain"]

    sl().driver.add_cookie(cookie_dict)


def user_should_be_at_top_of_page():
    (x, y) = sl().get_window_position()
    if y != 0:
        raise_assertion_error(f"Windows position Y is {y} not 0! User should be at the top of the page!")


def user_gets_row_number_with_heading(heading: str, table_locator: str = "css:table"):
    elem = get_child_element(table_locator, f'xpath:.//tbody/tr/th[text()="{heading}"]/..')
    rows = get_child_elements(table_locator, "css:tbody tr")
    return rows.index(elem) + 1


def user_gets_row_with_group_and_indicator(group: str, indicator: str, table_selector: str = "css:table"):
    table_elem = sl().get_webelement(table_selector)
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
    if element_finder().find(selector, required=False) is not None:
        sl().click_element(selector)


def user_is_on_admin_dashboard(admin_url: str) -> bool:
    current_url = sl().get_location()
    url_parts = urlparse(current_url)
    left_part = f"{url_parts.scheme}://{url_parts.netloc}{url_parts.path}"
    if left_part.endswith("/"):
        left_part = left_part[:-1]
    return left_part == admin_url or left_part == f"{admin_url}/dashboard"


def user_is_on_admin_dashboard_with_theme_selected(admin_url: str, theme: str) -> bool:
    if not user_is_on_admin_dashboard(admin_url):
        return False
    selected_theme = sl().get_selected_list_label("id:publicationsReleases-theme-themeId")
    if selected_theme != theme:
        return False


def user_navigates_to_admin_dashboard_if_needed(admin_url: str):
    if user_is_on_admin_dashboard(admin_url):
        return

    sl().go_to(admin_url)


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


def _get_webelement_from_locator(parent_locator: object, timeout: float = None, error: str = "") -> WebElement:
    if isinstance(parent_locator, str):
        sl().wait_until_page_contains_element(parent_locator, timeout=timeout, error=error)
        return sl().find_element(parent_locator)
    elif isinstance(parent_locator, WebElement):
        return parent_locator
    else:
        raise_assertion_error(f"Parent locator was neither a str or a WebElement - {parent_locator}")


def get_www_url(publicUrl: str):
    protocol, hostnameAndPort = publicUrl.split("://")

    return protocol + "://www." + hostnameAndPort


def remove_auth_from_url(publicUrl: str):
    parsed_url = urlparse(publicUrl)
    netloc = parsed_url.hostname

    if parsed_url.port:
        netloc += f":{parsed_url.port}"

    modified_url_without_auth = urlunparse(
        (parsed_url.scheme, netloc, parsed_url.path, parsed_url.params, parsed_url.query, parsed_url.fragment)
    )
    return modified_url_without_auth


def get_child_element_with_retry(parent_locator: object, child_locator: str, max_retries=3, retry_delay=2):
    def get_element():
        return get_child_element(parent_locator, child_locator)

    return do_with_retries(
        get_element, "get_child_element_with_retry", NoSuchElementException, max_retries, retry_delay
    )


def do_with_retries(action, action_description: str, allowed_exception_types, retries: int, retry_delay: int):
    last_exception = None
    retry_count = 0

    while retry_count < retries:
        try:
            return action()
        except allowed_exception_types as ex:
            last_exception = ex
            retry_count += 1
            time.sleep(retry_delay)

    raise_assertion_error(
        f"Failed to perform action {action_description} after {retries} retries."
        f"Got the following exception:\r\n\r\n{last_exception}"
    )
