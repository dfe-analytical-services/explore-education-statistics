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
        sl.wait_until_page_contains_element(parent_locator, timeout=timeout, error=error,
                                            limit=limit)

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


def user_checks_page_contains_accordion(accordion_heading):
    try:
        sl.wait_until_page_contains_element(
            f'xpath://*[@class="govuk-accordion__section-button" and text()="{accordion_heading}"]')
        # sl.driver.find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{accordion_heading}"]')
    except:
        raise_assertion_error(f"Accordion with heading '{accordion_heading}' not found!'")


def user_checks_accordion_is_in_position(header_starts_with, position):
    try:
        # NOTE(mark): When nth-child won't do, you need to do the unholy equivalent of css .class in xpath...
        elem = sl.driver.find_element_by_xpath(
            f'(.//*[contains(concat(" ", normalize-space(@class), " "), " govuk-accordion__section ")])[{position}]')
    except:
        raise_assertion_error(f"There are less than {position} accordion sections!")

    if not elem.text.strip().startswith(header_starts_with):
        raise_assertion_error(
            f'Accordion in position {position} expected start with text "{header_starts_with}". Actual found text: "{elem.text}"')


def user_checks_there_are_x_accordion_sections(num):
    sl.page_should_contain_element(
        'xpath:.//*[contains(concat(" ", normalize-space(@class), " "), " govuk-accordion__section ")]',
        limit=num)


def user_verifies_accordion_is_open(section_text):
    sl.wait_until_page_contains_element(
        f'xpath://*[@class="govuk-accordion__section-button" and text()="{section_text}"]')

    try:
        sl.driver \
            .find_element_by_xpath(
            f'//*[@class="govuk-accordion__section-button" and text()="{section_text}" and @aria-expanded="true"]')
    except NoSuchElementException:
        raise_assertion_error(
            f'Accordion section "{section_text}" should have attribute aria-expanded="true"')


def user_verifies_accordion_is_closed(section_text):
    sl.wait_until_page_contains_element(
        f'xpath://*[@class="govuk-accordion__section-button" and text()="{section_text}"]')
    try:
        sl.driver \
            .find_element_by_xpath(
            f'//*[@class="govuk-accordion__section-button" and text()="{section_text}"]')
    except NoSuchElementException:
        raise_assertion_error(f'Cannot find accordion with header {section_text}')

    try:
        sl.driver \
            .find_element_by_xpath(
            f'//*[@class="govuk-accordion__section-button" and text()="{section_text}" and @aria-expanded="false"]')
    except NoSuchElementException:
        raise_assertion_error(
            f'Accordion section "{section_text}" should have attribute aria-expanded="false"')


def user_opens_accordion_section(section_text):
    sl.wait_until_page_contains_element(
        f'xpath://*[@class="govuk-accordion__section-button" and text()="{section_text}"]')

    elem = user_gets_accordion_button_element(section_text)
    if elem.get_attribute('aria-expanded') == "true":
        raise_assertion_error(f'Accordion section "{section_text}" already open!')
    sl.set_focus_to_element(elem)
    sl.wait_until_element_is_enabled(elem)
    sl.click_element(elem)
    if elem.get_attribute('aria-expanded') == "false":
        raise_assertion_error(f'Accordion section "{section_text}" should be open!')


def user_closes_accordion_section(section_text):
    elem = user_gets_accordion_button_element(section_text)
    if elem.get_attribute('aria-expanded') == "false":
        raise_assertion_error(f'Accordion section "{section_text}" already closed!')
    sl.click_element(elem)
    if elem.get_attribute('aria-expanded') == "true":
        raise_assertion_error(f'Accordion section "{section_text}" should be open!')


def user_checks_accordion_section_contains_text(accordion_section_text, text):
    elem = user_gets_accordion_content_element(accordion_section_text)
    user_waits_until_parent_contains_element(elem, f'xpath://*[text()="{text}"]')


def user_gets_accordion_button_element(accordion_text):
    sl.wait_until_page_contains_element(
        f'xpath://button[@class="govuk-accordion__section-button" and text()="{accordion_text}"]')
    return sl.get_webelement(
        f'xpath://button[@class="govuk-accordion__section-button" and text()="{accordion_text}"]')


def user_gets_accordion_section_element(accordion_text):
    accordion_button = user_gets_accordion_button_element(accordion_text)
    heading_id = accordion_button.get_attribute("id")
    accordion_id = heading_id[:-len('-heading')]
    return sl.get_webelement(f'css:#{accordion_id}')


def user_gets_accordion_content_element(accordion_text):
    accordion_button = user_gets_accordion_button_element(accordion_text)
    heading_id = accordion_button.get_attribute("id")
    return sl.get_webelement(f'xpath://*[@aria-labelledby="{heading_id}"]')


def user_gets_child_details_element(parent, details_text):
    user_waits_until_parent_contains_element(parent,
                                             f'xpath:.//details/summary/*[text()="{details_text}"]/../..')
    return get_child_element(parent, f'xpath:.//details/summary/*[text()="{details_text}"]/../..')


def user_verifies_details_is_open(details_elem):
    if details_elem.find_element_by_xpath('..').get_attribute("aria-expanded") == "false":
        raise_assertion_error(f'Details component "{details_text}" not expanded!')


def user_closes_details_dropdown(details_text):
    elem = sl.get_webelement(f'xpath://details/summary/*[text()="{details_text}"]')
    sl.click_element(elem)
    if elem.find_element_by_xpath('..').get_attribute("aria-expanded") == "true":
        raise_assertion_error(f'Details component "{details_text}" is still expanded!')


def capture_large_screenshot():
    currentWindow = sl.get_window_size()
    page_height = sl.driver.execute_script(
        "return document.documentElement.scrollHeight;")

    page_width = currentWindow[0]
    original_height = currentWindow[1]

    sl.set_window_size(page_width, page_height)
    warn("Capturing a screenshot at URL " + sl.get_location())
    sl.capture_page_screenshot()
    sl.set_window_size(page_width, original_height)


def user_checks_previous_table_tool_step_contains(step, key, value, timeout=10):
    try:
        sl.wait_until_page_contains_element(
            f'xpath://*[@id="tableToolWizard-step-{step}"]//*[text()="Go to this step"]',
            timeout=timeout)
    except:
        raise_assertion_error(f'Previous step wasn\'t found!')

    try:
        sl.wait_until_page_contains_element(
            f'xpath://*[@id="tableToolWizard-step-{step}"]//dt[text()="{key}"]/..//*[text()="{value}"]',
            timeout=timeout)
    except:
        raise_assertion_error(
            f'Element "#tableToolWizard-step-{step}" containing "{key}" and "{value}" not found!')


def user_checks_table_column_heading_contains(table_selector, row, column, expected,
                                              timeout=30):
    user_waits_until_parent_contains_element(
        table_selector,
        f'xpath://thead/tr[{row}]/th[{column}][text()="{expected}"]',
        timeout=timeout)


def user_gets_table_row_with_heading(table_selector, heading: str):
    return get_child_element(table_selector, f'xpath:.//tbody/tr/th[text()="{heading}"]/..')


def user_gets_row_number_with_heading(table_locator: str, heading: str):
    elem = get_child_element(table_locator, f'xpath:.//tbody/tr/th[text()="{heading}"]/..')
    rows = get_child_elements(table_locator, 'css:tbody tr')

    return rows.index(elem) + 1


def user_gets_row_with_group_and_indicator(table_selector, group, indicator):
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


def user_checks_row_contains_heading(row_elem, heading):
    try:
        row_elem.find_element_by_xpath(f'.//th[text()="{heading}"]')
    except:
        raise_assertion_error(f'Heading "{heading}" not found for provided row element.')


def user_checks_row_cell_contains_text(row_elem, cell_num, expected_text):
    try:
        elem = row_elem.find_element_by_xpath(f'.//td[{cell_num}]')
    except:
        raise_assertion_error(f'Couldn\'t find TD tag num "{cell_num}" for provided row element')

    if expected_text not in elem.text:
        raise_assertion_error(
            f'TD tag num "{cell_num}" for row element didn\'t contain text "{expected_text}". Found text "{elem.text}"')


def user_checks_results_table_row_heading_contains(row, column, expected):
    elem = sl.driver.find_element_by_xpath(f'//table/tbody/tr[{row}]/th[{column}]')
    if expected not in elem.text:
        raise_assertion_error(
            f'"{expected}" not found in th tag in results table tbody row {row}, column {column}. Found text "{elem.text}".')


def user_checks_table_heading_in_offset_row_contains(table_locator: str, row: int,
                                                     offset: int, column: int,
                                                     expected: str):
    offset_row = int(row) + int(offset)
    elem = get_child_element(table_locator, f'xpath://tbody/tr[{offset_row}]/th[{column}]')

    if expected not in elem.text:
        raise_assertion_error(
            f'"{expected}" not found in th tag in results table tbody row {offset_row}, column {column}. Found text "{elem.text}".')


def user_checks_results_table_cell_contains(row, column, expected):
    elem = sl.driver.find_element_by_xpath(f'//table/tbody/tr[{row}]/td[{column}]')
    if expected not in elem.text:
        raise_assertion_error(
            f'"{expected}" not found in td tag in results table tbody row {row}, column {column}. Found text "{elem.text}".')


def user_checks_table_cell_in_offset_row_contains(table_locator: str, row: int, offset: int,
                                                  column: int, expected: str):
    offset_row = int(row) + int(offset)
    elem = get_child_element(table_locator, f'xpath://tbody/tr[{offset_row}]/td[{column}]')

    if expected not in elem.text:
        raise_assertion_error(
            f'"{expected}" not found in td tag in results table tbody row {offset_row}, column {column}. Found text "{elem.text}".')


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


def user_waits_until_details_dropdown_contains_publication(details_heading, publication_name,
                                                           timeout=3):
    sl.wait_until_page_contains_element(f'xpath://details/summary[.="{details_heading}"]',
                                        timeout=timeout)
    sl.wait_until_page_contains_element(
        f'xpath://details/summary[.="{details_heading}"]/../..//*[text()="{publication_name}"]')


def user_checks_details_dropdown_contains_download_link(details_heading, download_link):
    try:
        sl.driver.find_element_by_xpath(
            f'//*[contains(@class,"govuk-details__summary-text") and text()="{details_heading}"]')
    except:
        raise_assertion_error(f'Cannot find details component "{details_heading}"')

    try:
        sl.driver.find_element_by_xpath(
            f'//*[contains(@class,"govuk-details__summary-text") and text()="{details_heading}"]/../..//li/a[text()="{download_link}"]')
    except:
        raise_assertion_error(f'Cannot find link "{download_link}" in "{details_heading}"')


def remove_substring_from_right_of_string(string, substring):
    if string.endswith(substring):
        return string[:-len(substring)]
    else:
        raise_assertion_error(f'String "{string}" doesn\'t end with substring "{substring}"')
