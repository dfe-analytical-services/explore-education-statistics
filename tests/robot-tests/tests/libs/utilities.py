import json
import os
import re
import time
from typing import Union
from urllib.parse import urlparse, urlunparse

import utilities_init
from robot.libraries.BuiltIn import BuiltIn
from selenium.common.exceptions import NoSuchElementException
from selenium.webdriver.common.by import By
from selenium.webdriver.remote.webelement import WebElement
from SeleniumLibrary.utils import is_noney
from tests.libs.logger import get_logger
from tests.libs.selenium_elements import element_finder, sl, waiting

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


def user_waits_until_parent_contains_element(
    parent_locator_or_element: object,
    child_locator: str,
    timeout: int = None,
    error: str = None,
    count: int = None,
    retries: int = 5,
    delay: float = 1.0,
):
    try:
        default_timeout = BuiltIn().get_variable_value("${TIMEOUT}")
        timeout_per_retry = timeout / retries if timeout is not None else int(default_timeout) / retries

        child_locator = _normalise_child_locator(child_locator)

        def parent_contains_matching_element() -> bool:
            parent_el = _get_webelement_from_locator(parent_locator_or_element, timeout_per_retry, error)
            return element_finder().find(child_locator, required=False, parent=parent_el) is not None

        if is_noney(count):
            return retry_or_fail_with_delay(
                waiting()._wait_until,
                retries,
                delay,
                parent_contains_matching_element,
                "Parent '%s' did not contain '%s' in <TIMEOUT>." % (parent_locator_or_element, child_locator),
                timeout_per_retry,
                error,
            )

        count = int(count)

        def parent_contains_matching_elements() -> bool:
            parent_el = _get_webelement_from_locator(parent_locator_or_element, timeout_per_retry, error)
            return len(sl().find_elements(child_locator, parent=parent_el)) == count

        retry_or_fail_with_delay(
            waiting()._wait_until,
            retries,
            delay,
            parent_contains_matching_elements,
            "Parent '%s' did not contain %s '%s' element(s) within <TIMEOUT>."
            % (parent_locator_or_element, count, child_locator),
            timeout_per_retry,
            error,
        )
    except Exception as err:
        logger.warning(
            f"Error whilst executing utilities.py user_waits_until_parent_contains_element() "
            f"with parent {parent_locator_or_element} and child locator {child_locator} - {err}"
        )
        raise_assertion_error(err)


def user_waits_until_parent_contains_element_without_retries(
    parent_locator: object, child_locator: str, timeout: int = None, error: str = None, count: int = None
):
    try:
        child_locator = _normalise_child_locator(child_locator)

        def parent_contains_matching_element() -> bool:
            parent_el = _get_webelement_from_locator(parent_locator, timeout, error)
            return element_finder().find(child_locator, required=False, parent=parent_el) is not None

        if is_noney(count):
            return waiting()._wait_until(
                parent_contains_matching_element,
                "Parent '%s' did not contain '%s' in <TIMEOUT>." % (parent_locator, child_locator),
                timeout,
                error,
            )

        count = int(count)

        def parent_contains_matching_elements() -> bool:
            parent_el = _get_webelement_from_locator(parent_locator, timeout, error)
            return len(sl().find_elements(child_locator, parent=parent_el)) == count

        waiting()._wait_until(
            parent_contains_matching_elements,
            "Parent '%s' did not contain %s '%s' element(s) within <TIMEOUT>." % (parent_locator, count, child_locator),
            timeout,
            error,
        )
    except Exception as err:
        logger.warning(
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
            parent_el = _get_webelement_from_locator(parent_locator, timeout, error)
            return element_finder().find(child_locator, required=False, parent=parent_el) is None

        if is_noney(count):
            return waiting()._wait_until(
                parent_does_not_contain_matching_element,
                "Parent '%s' should not have contained '%s' in <TIMEOUT>." % (parent_locator, child_locator),
                timeout,
                error,
            )

        count = int(count)

        def parent_does_not_contain_matching_elements() -> bool:
            parent_el = _get_webelement_from_locator(parent_locator, timeout, error)
            return len(sl().find_elements(child_locator, parent=parent_el)) != count

        waiting()._wait_until(
            parent_does_not_contain_matching_elements,
            "Parent '%s' should not have contained %s '%s' element(s) within <TIMEOUT>."
            % (parent_locator, count, child_locator),
            timeout,
            error,
        )
    except Exception as err:
        logger.warning(
            f"Error whilst executing utilities.py "
            f"user_waits_until_parent_does_not_contain_element() with parent {parent_locator} "
            f"and child locator {child_locator} - {err}"
        )
        raise_assertion_error(err)


def get_child_element(parent_locator: object, child_locator: str, retries: int = 5, delay: float = 1.0):
    for attempt in range(retries):
        try:
            children = get_child_elements(parent_locator, child_locator)
            if not children:
                if attempt < retries - 1:
                    logger.info(f"Retrying... ({attempt + 1}/{retries})")
                    time.sleep(delay)
                    continue
                else:
                    raise_assertion_error(
                        f"No elements matching child locator '{child_locator}' under parent locator '{parent_locator}' after {retries} retries"
                    )
            if len(children) > 1:
                logger.warning(
                    f"Multiple ({len(children)}) elements found for child locator '{child_locator}' under parent locator '{parent_locator}'. "
                    f"Returning the first element. Consider refining the parent selector."
                )
            return children[0]

        except Exception as err:
            if attempt < retries - 1:
                logger.info(f"Retrying due to error... ({attempt + 1}/{retries})")
                time.sleep(delay)
                continue
            else:
                raise_assertion_error(
                    f"Error in get_child_element() with parent '{parent_locator}' and child locator '{child_locator}': {err}"
                )


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
    element = _get_webelement_from_locator(locator_or_element)
    sl().driver.execute_script('arguments[0].scrollIntoView({behavior: "instant", block: "center"})', element)


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


def _get_webelement_from_locator(parent_locator: object, timeout: int = None, error: str = "") -> WebElement:
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
    retry_count = 0
    while retry_count < max_retries:
        try:
            return get_child_element(parent_locator, child_locator)
        except NoSuchElementException:
            retry_count += 1
            logger.warning(f"Child element not found, after ({max_retries}) retries")
            time.sleep(retry_delay)
    raise AssertionError(f"Failed to find child element after {max_retries} retries.")
