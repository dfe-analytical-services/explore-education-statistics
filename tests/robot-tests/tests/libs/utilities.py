import json

import pytz
import time
import re
import datetime
from logging import warn

from SeleniumLibrary.utils import is_noney
from robot.libraries.BuiltIn import BuiltIn
from SeleniumLibrary import ElementFinder
from SeleniumLibrary.errors import ElementNotFound
from SeleniumLibrary.keywords.waiting import WaitingKeywords
from selenium.webdriver.common.keys import Keys
from selenium.common.exceptions import NoSuchElementException
import os

sl = BuiltIn().get_library_instance('SeleniumLibrary')
element_finder = ElementFinder(sl)
waiting = WaitingKeywords(sl)


def raise_assertion_error(err_msg):
    sl.failure_occurred()
    raise AssertionError(err_msg)


def user_waits_until_parent_contains_element(parent_locator: str, child_locator: str,
                                             timeout: int = None, error: str = None,
                                             limit: int = None):
    try:
        sl.wait_until_page_contains_element(parent_locator, timeout=timeout, error=error)

        def parent_contains_matching_element() -> bool:
            parent_el = sl.find_element(parent_locator)
            return element_finder.find(child_locator, required=False, parent=parent_el) is not None

        if is_noney(limit):
            return waiting._wait_until(
                parent_contains_matching_element,
                "Parent '%s' did not contain '%s' in <TIMEOUT>." % (parent_locator, child_locator),
                timeout, error
            )

        limit = int(limit)

        def parent_contains_matching_elements() -> bool:
            parent_el = sl.find_element(parent_locator)
            return len(sl.find_elements(child_locator, parent=parent_el)) == limit

        waiting._wait_until(
            parent_contains_matching_elements,
            "Parent '%s' did not contain %s '%s' element(s) within <TIMEOUT>." % (
                parent_locator, limit, child_locator),
            timeout, error
        )
    except Exception as err:
        raise_assertion_error(err)


def user_waits_until_parent_does_not_contain_element(parent_locator: str, child_locator: str,
                                                     timeout: int = None, error: str = None,
                                                     limit: int = None):
    try:
        sl.wait_until_page_contains_element(parent_locator, timeout=timeout, error=error,
                                            limit=limit)

        def parent_does_not_contain_matching_element() -> bool:
            parent_el = sl.find_element(parent_locator)
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
            parent_el = sl.find_element(parent_locator)
            return len(sl.find_elements(child_locator, parent=parent_el)) != limit

        waiting._wait_until(
            parent_does_not_contain_matching_elements,
            "Parent '%s' should not have contained %s '%s' element(s) within <TIMEOUT>." % (
                parent_locator, limit, child_locator),
            timeout, error
        )
    except Exception as err:
        raise_assertion_error(err)


def get_child_element(parent_locator: str, child_locator: str):
    parent_el = None

    try:
        parent_el = sl.find_element(parent_locator)
    except Exception as err:
        raise_assertion_error(err)

    try:
        return sl.find_element(child_locator, parent=parent_el)
    except ElementNotFound:
        raise_assertion_error(
            f"Could not find child '{child_locator}' within parent '{parent_locator}'")
    except Exception as err:
        raise_assertion_error(err)


def get_child_elements(parent_locator: str, child_locator: str):
    try:
        parent_el = sl.find_element(parent_locator)
        return sl.find_elements(child_locator, parent=parent_el)
    except Exception as err:
        raise_assertion_error(err)


def user_waits_for_page_to_finish_loading():
    # This is required because despite the DOM being loaded, and even a button being enabled, React/NextJS
    # hasn't finished processing the page, and so click are intermittently ignored. I'm wrapping
    # this sleep in a keyword such that if we find a way to check whether the JS processing has finished in the
    # future, we can change it here.
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
        raise_assertion_error(
            f"Windows position Y is {y} not 0! User should be at the top of the page!")

def capture_large_screenshot_and_prompt_to_continue():
    capture_large_screenshot()
    warn("Failure encountered - continue? (Y/n)")
    choice=input()
    if (choice.lower().startsWith("n")):
        raise_assertion_error('Test failed and you chose to stop the tests')


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

    warn(f"Captured a screenshot at URL {sl.get_location()}     Screenshot saved to file://{screenshot_location}")


def capture_html():
    html = sl.get_source()
    current_time_millis=round(datetime.datetime.timestamp(datetime.datetime.now()) * 1000)
    html_file = open(f"test-results/captured-html-{current_time_millis}.html", "w")
    html_file.write(html)
    html_file.close()
    warn(f"Captured HTML of {sl.get_location()}      HTML saved to file://{os.path.realpath(html_file.name)}")

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
            f'TD tag num "{cell_num}" for row element didn\'t contain text "{expected_text}". Found text "{elem.text}"')


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

def user_chooses_what_to_do_on_step_failure(selector):
    if element_finder.find(selector, required=False) is not None:
        sl.click_element(selector)
