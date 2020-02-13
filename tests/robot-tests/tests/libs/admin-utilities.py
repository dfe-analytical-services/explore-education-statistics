from robot.libraries.BuiltIn import BuiltIn
sl = BuiltIn().get_library_instance('SeleniumLibrary')

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
