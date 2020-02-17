from robot.libraries.BuiltIn import BuiltIn
sl = BuiltIn().get_library_instance('SeleniumLibrary')
import time

def data_csv_number_contains_xpath(num, xpath):
    try:
        elem = sl.driver.find_element_by_xpath(f'//*[@id="dataFileUploadForm"]/dl[{num}]')
    except:
        raise AssertionError(f'Cannot find data file number "{num}"')
    try:
        elem.find_element_by_xpath(xpath)
    except:
        raise AssertionError(f'Cannot find data file number "{num} with xpath {xpath}')

def data_file_number_contains_xpath(num, xpath):
    try:
        elem = sl.driver.find_element_by_xpath(f'//*[@id="fileUploadForm"]/dl[{num}]')
    except:
        raise AssertionError(f'Cannot find data file number "{num}"')
    try:
        elem.find_element_by_xpath(xpath)
    except:
        raise AssertionError(f'Cannot find data file number "{num} with xpath {xpath}')

def user_changes_accordion_section_title(num, new_title):
    try:
        elem = sl.driver.find_element_by_xpath(f'//*[@id="contents-accordion"]/div[contains(@class, "govuk-accordion__section")][{num}]')
    except:
        raise AssertionError(f'Cannot find accordion section number "{num}"')

    try:
        elem.find_element_by_xpath('.//a[.="(Edit Section Title)"]').click()
    except:
        raise AssertionError('Cannot click "(Edit Section Title)" link!')

    try:
        elem.find_element_by_xpath('.//input[@id="heading"]').clear()
    except:
        raise AssertionError('Failed to clear accordion section title text input field')

    try:
        sl.press_keys(elem.find_element_by_xpath('.//input[@id="heading"]'), new_title)
    except:
        raise AssertionError('Failed to press keys!?!')

    try:
        elem.find_element_by_xpath('.//a[.="(Save Section Title)"]').click()
    except:
        raise AssertionError('Cannot click "(Save Section Title)" link!')

def user_gets_editable_accordion_section_element(section_title):
    try:
        elem = sl.driver.find_element_by_xpath(f'//*[@id="contents-accordion"]//span[text()="{section_title}"]/../../..')
    except:
        raise AssertionError(f'Cannot find accordion section titled "{section_title}"')

    return elem

def user_opens_editable_accordion_section(section_elem):
    try:
        section_elem.click()
    except:
        raise AssertionError('Cannot click accordion section element')

def user_clicks_add_content_for_editable_accordion_section(section_elem, timeout=10):
    try:
        section_elem.find_element_by_xpath('.//button[text()="Add content"]').click()
    except:
        raise AssertionError(f'Failed to click "Add content" button for accordion section element')

    max_time = time.time() + timeout
    while time.time() < max_time:
        try:
            section_elem.find_element_by_xpath('.//p[text()="This section is empty"]')
            return
        except:
            time.sleep(0.5)
    raise AssertionError('Failed to find new empty content section')

def user_adds_content_to_accordion_section_content_block(section_elem, block_num, content):
    try:
        section_elem.find_element_by_xpath(f'(.//span[text()="Edit"])[{block_num}]').click()
    except:
        raise AssertionError(f'Failed to find Delete button for content block number {block_num}')

    sl.log_to_console('Content: ', content)
    sl.press_keys(content)

def user_deletes_editable_accordion_section_content_block(section_elem, block_num):
    try:
        section_elem.find_element_by_xpath(f'(.//button[text()="Delete"])[{block_num}]').click()
    except:
        raise AssertionError(f'Failed to find/click Delete button for content block number {block_num}')

    try:
        sl.driver.find_element_by_xpath(f'//button[text()="Confirm"]').click()
    except:
        raise AssertionError('Failed to find/click Confirm button')
