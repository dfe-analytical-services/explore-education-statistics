import json
import pytz
import time
import datetime
from logging import warning
from SeleniumLibrary.utils import is_noney
from robot.libraries.BuiltIn import BuiltIn
from SeleniumLibrary.keywords.waiting import WaitingKeywords
from selenium.webdriver.remote.webelement import WebElement
from typing import Union
import utilities_init
import os
import re

sl = BuiltIn().get_library_instance('SeleniumLibrary')
element_finder = sl._element_finder
waiting = WaitingKeywords(sl)

# Should only initialise some parts once e.g. registration
# of custom locators onto the framework's ElementFinder
if not utilities_init.initialised:
    def _normalize_parent_locator(parent_locator: object) -> Union[str, WebElement]:
        if not isinstance(parent_locator, str) and not isinstance(parent_locator, WebElement):
            return 'css:body'

        return parent_locator

    def _find_by_label(parent_locator: object, criteria: str, tag: str, constraints: dict) -> list:
        parent_locator = _normalize_parent_locator(parent_locator)

        labels = get_child_elements(parent_locator, f'xpath:.//label[text()="{criteria}"]')

        if len(labels) == 0:
            return []

        for_id = labels[0].get_attribute('for')
        return get_child_elements(parent_locator, f'id:{for_id}')


    def _find_by_testid(parent_locator: object, criteria: str, tag: str, constraints: dict) -> list:
        parent_locator = _normalize_parent_locator(parent_locator)

        return get_child_elements(parent_locator, f'css:[data-testid="{criteria}"]')

    # Register locator strategies

    element_finder.register('label', _find_by_label, persist=True)
    element_finder.register('testid', _find_by_testid, persist=True)

    utilities_init.initialised = True


def raise_assertion_error(err_msg):
    sl.failure_occurred()
    raise AssertionError(err_msg)


def user_waits_until_parent_contains_element(parent_locator: object, child_locator: str,
                                             timeout: int = None, error: str = None,
                                             limit: int = None):
    try:
        child_locator = _normalise_child_locator(parent_locator, child_locator)

        def parent_contains_matching_element() -> bool:
            parent_el = _get_parent_webelement_from_locator(parent_locator, timeout, error)
            return element_finder.find(child_locator, required=False, parent=parent_el) is not None

        if is_noney(limit):
            return waiting._wait_until(
                parent_contains_matching_element,
                "Parent '%s' did not contain '%s' in <TIMEOUT>." % (parent_locator, child_locator),
                timeout, error
            )

        limit = int(limit)

        def parent_contains_matching_elements() -> bool:
            parent_el = _get_parent_webelement_from_locator(parent_locator, timeout, error)
            return len(sl.find_elements(child_locator, parent=parent_el)) == limit

        waiting._wait_until(
            parent_contains_matching_elements,
            "Parent '%s' did not contain %s '%s' element(s) within <TIMEOUT>." % (
                parent_locator, limit, child_locator),
            timeout, error
        )
    except Exception as err:
        warning(f"Error whilst executing utilities.py user_waits_until_parent_contains_element() "
                f"with parent {parent_locator} and child locator {child_locator} - {err}")
        raise_assertion_error(err)


def user_waits_until_parent_does_not_contain_element(parent_locator: object, child_locator: str,
                                                     timeout: int = None, error: str = None,
                                                     limit: int = None):
    try:
        child_locator = _normalise_child_locator(parent_locator, child_locator)

        def parent_does_not_contain_matching_element() -> bool:
            parent_el = _get_parent_webelement_from_locator(parent_locator, timeout, error)
            return element_finder.find(child_locator, required=False, parent=parent_el) is None

        if is_noney(limit):
            return waiting._wait_until(
                parent_does_not_contain_matching_element,
                "Parent '%s' should not have contained '%s' in <TIMEOUT>." % (
                parent_locator, child_locator),
                timeout, error
            )

        limit = int(limit)

        def parent_does_not_contain_matching_elements() -> bool:
            parent_el = _get_parent_webelement_from_locator(parent_locator, timeout, error)
            return len(sl.find_elements(child_locator, parent=parent_el)) != limit

        waiting._wait_until(
            parent_does_not_contain_matching_elements,
            "Parent '%s' should not have contained %s '%s' element(s) within <TIMEOUT>." % (
                parent_locator, limit, child_locator),
            timeout, error
        )
    except Exception as err:
        warning(f"Error whilst executing utilities.py "
                f"user_waits_until_parent_does_not_contain_element() with parent {parent_locator} "
                f"and child locator {child_locator} - {err}")
        raise_assertion_error(err)


def get_child_element(parent_locator: object, child_locator: str):
    try:
        children = get_child_elements(parent_locator, child_locator)

        if len(children) > 1:
            warning(f"Found {len(children)} child elements matching child locator {child_locator} "
                    f"under parent locator {parent_locator} in utilities.py#get_child_element() - "
                    f"was expecting only one. Consider making the parent selector more specific. "
                    f"Returning the first element found.")

        return children[0]
    except Exception as err:
        warning(f"Error whilst executing utilities.py get_child_element() with parent {parent_locator} and child "
                f"locator {child_locator} - {err}")
        raise_assertion_error(err)


def get_child_elements(parent_locator: object, child_locator: str):
    try:
        child_locator = _normalise_child_locator(parent_locator, child_locator)
        parent_el = _get_parent_webelement_from_locator(parent_locator)
        return element_finder.find_elements(child_locator, parent=parent_el)
    except Exception as err:
        warning(f"Error whilst executing utilities.py get_child_elements() - {err}")
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
    del cookie_dict['domain']

    sl.driver.add_cookie(cookie_dict)


def format_uk_to_local_datetime(uk_local_datetime: str, strf: str) -> str:
    if os.name == 'nt':
        strf = strf.replace('%-', '%#')

    tz = pytz.timezone('Europe/London')

    return tz.localize(datetime.datetime.fromisoformat(uk_local_datetime)) \
        .astimezone().strftime(strf)


def get_current_datetime(strf: str, offset_days: int = 0) -> str:
    now = datetime.datetime.now() + datetime.timedelta(days=offset_days)

    if os.name == 'nt':
        strf = strf.replace('%-', '%#')

    return now.strftime(strf)


def user_should_be_at_top_of_page():
    (x, y) = sl.get_window_position()
    if y != 0:
        raise_assertion_error(f"Windows position Y is {y} not 0! User should be at the top of the page!")


def prompt_to_continue():
    warning("Continue? (Y/n)")
    choice = input()
    if (choice.lower().startswith("n")):
        raise_assertion_error('Tests stopped!')


def capture_large_screenshot_and_prompt_to_continue():
    capture_large_screenshot()
    prompt_to_continue()


def capture_large_screenshot_and_html():
    capture_large_screenshot()
    capture_html()


def capture_large_screenshot():
    currentWindow = sl.get_window_size()
    page_height = sl.driver.execute_script(
        "return document.documentElement.scrollHeight;")

    page_width = currentWindow[0]
    original_height = currentWindow[1]

    sl.set_window_size(page_width, page_height)
    screenshot_location = sl.capture_page_screenshot()
    sl.set_window_size(page_width, original_height)

    warning(f"Captured a screenshot at URL {sl.get_location()}     Screenshot saved to file://{screenshot_location}")


def capture_html():
    html = sl.get_source()
    current_time_millis=round(datetime.datetime.timestamp(datetime.datetime.now()) * 1000)
    html_file = open(f"test-results/captured-html-{current_time_millis}.html", "w")
    html_file.write(html)
    html_file.close()
    warning(f"Captured HTML of {sl.get_location()}      HTML saved to file://{os.path.realpath(html_file.name)}")


def user_gets_row_number_with_heading(heading: str, table_locator: str = 'css:table'):
    elem = get_child_element(table_locator, f'xpath:.//tbody/tr/th[text()="{heading}"]/..')
    rows = get_child_elements(table_locator, 'css:tbody tr')

    return rows.index(elem) + 1


def user_gets_row_with_group_and_indicator(group: str, indicator: str,
                                           table_selector: str = 'css:table'):
    table_elem = sl.get_webelement(table_selector)
    elems = table_elem.find_elements_by_xpath(
        f'.//tbody/tr/th[text()="{group}"]/../self::tr | .//tbody/tr/th[text()="{group}"]/../following-sibling::tr')
    for elem in elems:
        try:
            elem.find_element_by_xpath(f'.//th[text()="{indicator}"]/..')
            return elem
        except:
            continue
    raise_assertion_error(f'Indicator "{indicator}" not found!')


def user_checks_row_cell_contains_text(row_elem, cell_num, expected_text):
    try:
        elem = get_child_element(row_elem, f'xpath:.//td[{cell_num}]')
    except:
        raise_assertion_error(f'Couldn\'t find TD tag num "{cell_num}" for provided row element')

    if expected_text not in elem.text:
        raise_assertion_error(
            f'TD tag num "{cell_num}" for row element didn\'t contain text "{expected_text}". '
            f'Found text "{elem.text}"')


def user_checks_list_contains_x_elements(list_locator, num):
    labels = sl.get_list_items(list_locator)
    if len(labels) != int(num):
        raise_assertion_error(f'Found {len(labels)} in list, not {num}. Locator: "{list_locator}"')


def user_checks_list_contains_at_least_x_elements(list_locator, num):
    labels = sl.get_list_items(list_locator)
    if len(labels) < int(num):
        raise_assertion_error(f'Found {len(labels)} in list, not {num}. Locator: "{list_locator}"')


def user_checks_list_contains_label(list_locator, label):
    labels = sl.get_list_items(list_locator)
    if label not in labels:
        raise_assertion_error(
            f'"{label}" wasn\'t found amongst list items "{labels}" from locator "{list_locator}"')


def user_checks_list_does_not_contain_label(list_locator, label):
    labels = sl.get_list_items(list_locator)
    if label in labels:
        raise_assertion_error(
            f'"{label}" was found amongst list items "{labels}" from locator "{list_locator}"')


def user_checks_selected_list_label(list_locator, label):
    selected_label = sl.get_selected_list_label(list_locator)
    if selected_label != label:
        raise_assertion_error(
            f'Selected label "{selected_label}" didn\'t match label "{label}" for list "{list_locator}"')


def remove_substring_from_right_of_string(string, substring):
    if string.endswith(substring):
        return string[:-len(substring)]
    else:
        raise_assertion_error(f'String "{string}" doesn\'t end with substring "{substring}"')


def user_clicks_element_if_exists(selector):
    if element_finder.find(selector, required=False) is not None:
        sl.click_element(selector)


def _normalise_child_locator(parent_locator: object, child_locator: str) -> str:
    if isinstance(parent_locator, str):
        return child_locator
    elif isinstance(parent_locator, WebElement):
        # the below substitution is necessary if the parent is a Selenium WebElement in order to
        # correctly find the parent's descendants.  Without the preceding dot, the double forward
        # slash breaks out of the parent container and returns the xpath query to the root of the
        # DOM, leading to false positives or incorrectly found DOM elements.  The below
        # substitution covers both selectors beginning with "xpath://" and "//", as the double
        # forward slashes without the "xpath:" prefix are inferred as being xpath expressions.
        return re.sub(r'^(xpath:)?//', "xpath:.//", child_locator)
    else:
        raise_assertion_error(f"Parent locator was neither a str or a WebElement - {parent_locator}")


def _get_parent_webelement_from_locator(parent_locator: object, timeout: int = None, error: str = '') -> WebElement:
    if isinstance(parent_locator, str):
        sl.wait_until_page_contains_element(parent_locator, timeout=timeout, error=error)
        return sl.find_element(parent_locator)
    elif isinstance(parent_locator, WebElement):
        return parent_locator
    else:
        raise_assertion_error(f"Parent locator was neither a str or a WebElement - {parent_locator}")
