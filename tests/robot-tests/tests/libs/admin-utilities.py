from selenium.webdriver.common.keys import Keys
from robot.libraries.BuiltIn import BuiltIn

sl = BuiltIn().get_library_instance('SeleniumLibrary')
import time
import os
import json
import requests


def raise_assertion_error(err_msg):
    sl.capture_page_screenshot()
    raise AssertionError(err_msg)


def get_release_guid_from_release_status_page_url(url):
    assert url.endswith('/status')
    url_components = url.split('/')
    return url_components[-2]


def user_triggers_release_on_demand(release_id):
    release_on_demand_endpoint = f'{os.getenv("ADMIN_URL")}/api/bau/release/{release_id}/publish'
    jwt_token = json.loads(os.environ['IDENTITY_LOCAL_STORAGE_BAU'])['access_token']
    headers = {
        'Content-Type': 'application/json',
        'Authorization': f'Bearer {jwt_token}',
    }

    resp = requests.put(release_on_demand_endpoint, headers=headers, verify=False)
    assert resp.status_code == 200, f'Release on demand request failed! Returned response code {resp.status_code}'


def data_csv_number_contains_xpath(num, xpath):
    try:
        elem = sl.driver.find_element_by_xpath(f'//*[@id="dataFileUploadForm"]/dl[{num}]')
    except:
        raise_assertion_error(f'Cannot find data file number "{num}"')
    try:
        elem.find_element_by_xpath(xpath)
    except:
        raise_assertion_error(f'Cannot find data file number "{num} with xpath {xpath}')


def data_file_number_contains_xpath(num, xpath):
    try:
        elem = sl.driver.find_element_by_xpath(f'//*[@id="fileUploadForm"]/dl[{num}]')
    except:
        raise_assertion_error(f'Cannot find data file number "{num}"')
    try:
        elem.find_element_by_xpath(xpath)
    except:
        raise_assertion_error(f'Cannot find data file number "{num} with xpath {xpath}')


def user_changes_accordion_section_title(num, new_title):
    sl.wait_until_page_contains_element(f'xpath://*[@data-testid="EditableAccordionSection"][{num}]')
    try:
        elem = sl.driver.find_element_by_xpath(f'//*[@data-testid="EditableAccordionSection"][{num}]')
    except:
        raise_assertion_error(f'Cannot find accordion section number "{num}"')

    try:
        elem.find_element_by_xpath('.//h2[contains(@class, "govuk-accordion__section-heading")]/button').click()
    except:
        raise_assertion_error(f'Cannot open accordion section number "{num}"')

    try:
        elem.find_element_by_xpath('.//button[.="Edit section title"]').click()
    except:
        raise_assertion_error('Cannot click "Edit section title" button!')

    sl.press_keys(elem.find_element_by_xpath('.//input[@id="heading"]'), 'CTRL+a+BACKSPACE')
    sl.press_keys(elem.find_element_by_xpath('.//input[@id="heading"]'), f'{new_title}')

    try:
        elem.find_element_by_xpath('.//button[.="Save section title"]').click()
    except:
        raise_assertion_error('Cannot click "Save section title" button!')

    sl.wait_until_page_contains_element(
        f'xpath://*[@data-testid="EditableAccordionSection"][{num}]//button[.="Edit section title"]')

    try:
        elem.find_element_by_xpath(f'.//button[text()="{new_title}"]').click()
    except:
        raise_assertion_error(f'Cannot close accordion section number "{num}"')


def user_gets_editable_accordion_section_element(section_title):
    try:
        elem = sl.driver.find_element_by_xpath(
            f'//*[@data-testid="EditableAccordionSection"][.//button = "{section_title}"]')
    except:
        raise_assertion_error(f'Cannot find accordion section titled "{section_title}"')
    return elem


def user_opens_editable_accordion_section(section_elem):
    try:
        section_elem.click()
    except:
        raise_assertion_error('Cannot click accordion section element')


def user_adds_text_block_to_editable_accordion_section(section_elem, timeout=30):
    try:
        elem = section_elem.find_element_by_xpath('.//button[text()="Add text block"]')
    except Exception as e:
        raise_assertion_error(
            f'Failed to get "Add text block" button element for accordion section element\nException: ', e)

    max_time = time.time() + timeout
    while (not elem.is_enabled()) and (time.time() < max_time):
        time.sleep(0.5)
    if time.time() >= max_time:
        raise_assertion_error(
            f'Failed to click "Add text block" button for accordion section element. It wasn\'t enabled!')

    try:
        elem.click()
    except Exception as e:
        raise_assertion_error(f'Failed to click "Add text block" button for accordion section element\nException: ', e)

    max_time = time.time() + timeout
    while time.time() < max_time:
        try:
            section_elem.find_element_by_xpath('.//p[text()="This section is empty"]')
            return
        except:
            time.sleep(0.5)
    raise_assertion_error('Failed to find new empty content section')


def user_checks_accordion_section_contains_X_blocks(section_elem, num_blocks):
    try:
        elems = section_elem.find_elements_by_xpath(
            f'.//*[@data-testid="EditableSectionBlock"]'
        )
    except:
        raise_assertion_error(f'Failed to find any content blocks in accordion section')

    assert len(elems) == int(num_blocks), f'Found {len(elems)} content blocks. Should have found {num_blocks}'


def user_adds_content_to_accordion_section_text_block(section_elem, block_num, content):
    try:
        section_elem.find_element_by_xpath(f'(.//button[text()="Edit block"])[{block_num}]').click()
    except:
        raise_assertion_error(f'Failed to find "Edit block" button for content block number {block_num}')

    sl.press_keys(None, content)

    try:
        section_elem.find_element_by_xpath(f'.//button[text()="Save"]').click()
    except:
        raise_assertion_error(f'Failed to find Save button for content block')

    sl.wait_until_page_contains_element(f'//p[text()="{content}"]')


def user_checks_accordion_section_text_block_contains_text(section_elem, block_num, content):
    try:
        section_elem.find_element_by_xpath(
            f'.//*[@data-testid="EditableSectionBlock"][{block_num}]//*[text()="{content}"]'
        )
    except:
        raise_assertion_error(f'Failed to find text "{content}" in content block number {block_num}')


def user_deletes_editable_accordion_section_content_block(section_elem, block_num):
    try:
        section_elem.find_element_by_xpath(f'(.//button[text()="Remove block"])[{block_num}]').click()
    except:
        raise_assertion_error(f'Failed to find Delete button for content block number {block_num}')

    try:
        sl.driver.find_element_by_xpath(f'//button[text()="Confirm"]').click()
    except:
        raise_assertion_error('Failed to find/click Confirm button')


def user_waits_for_release_process_status_to_be(status, timeout):
    max_time = time.time() + int(timeout)
    while time.time() < max_time:
        try:
            sl.driver.find_element_by_css_selector(f'#release-process-status-{status}')
            return
        except:
            sl.reload_page()
            time.sleep(10)
    raise_assertion_error(f'Release process status wasn\'t {status} after {timeout} seconds!')
