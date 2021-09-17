from robot.libraries.BuiltIn import BuiltIn
import time
import os
from tests.libs.setup_auth_variables import setup_auth_variables
from tests.libs.utilities import set_to_local_storage
from tests.libs.utilities import set_cookie_from_json

sl = BuiltIn().get_library_instance('SeleniumLibrary')


def raise_assertion_error(err_msg):
    sl.failure_occurred()
    raise AssertionError(err_msg)


def user_signs_in_as(user: str):
    try:
        (local_storage_token, cookie_token) = setup_auth_variables(
            user,
            email=os.getenv(f'{user}_EMAIL'),
            password=os.getenv(f'{user}_PASSWORD'),
            driver=sl.driver
        )

        admin_url = os.getenv('ADMIN_URL')
        assert admin_url

        set_to_local_storage(
            f'GovUk.Education.ExploreEducationStatistics.Adminuser:{admin_url}:GovUk.Education.ExploreEducationStatistics.Admin',
            local_storage_token
        )
        set_cookie_from_json(cookie_token)

        sl.go_to(admin_url)
    except Exception as e:
        raise_assertion_error(e)

# TODO EES-2302 / EES-2305 - Remove me when methodology notes can be added via content page
def get_methodology_version_id_from_url():
    url = sl.get_location()
    assert '/methodology/' in url, 'URL does not contain /methodology'
    result = url[len(os.getenv('ADMIN_URL')):].lstrip('/').split('/')
    assert result[0] == 'methodology', 'String "methodology" should be 1st element in list'
    return result[1]

def get_theme_id_from_url():
    url = sl.get_location()
    assert '/themes/' in url, 'URL does not contain /themes'
    result = url[len(os.getenv('ADMIN_URL')):].lstrip('/').split('/')
    assert result[0] == 'themes', 'String "themes" should be 1st element in list'
    return result[1]


def get_release_guid_from_release_status_page_url(url):
    assert url.endswith('/status')
    url_components = url.split('/')
    return url_components[-2]


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


def user_waits_for_release_process_status_to_be(status, timeout):
    max_time = time.time() + int(timeout)
    while time.time() < max_time:
        try:
            sl.driver.find_element_by_css_selector(f'#release-process-status-{status}')
            return
        except:
            sl.reload_page()  # Necessary if release previously scheduled
            time.sleep(1)
    raise_assertion_error(f'Release process status wasn\'t {status} after {timeout} seconds!')
