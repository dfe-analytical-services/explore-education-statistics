from selenium.webdriver.common.keys import Keys
from robot.libraries.BuiltIn import BuiltIn
import time
import os
import json
import requests
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


def admin_request(method, endpoint, body=None):
    assert method and endpoint
    assert os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN') is not None
    assert os.getenv('ADMIN_URL') is not None

    # To prevent InsecureRequestWarning
    requests.packages.urllib3.disable_warnings()

    jwt_token = json.loads(os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN'))['access_token']
    headers = {
        'Content-Type': 'application/json',
        'Authorization': f'Bearer {jwt_token}',
    }
    return requests.request(
        method,
        url=f'{os.getenv("ADMIN_URL")}{endpoint}',
        headers=headers,
        json=body,
        verify=False
    )


def delete_theme(theme_id: str):
    assert theme_id

    resp = admin_request('DELETE', f'/api/themes/{theme_id}')
    assert resp.status_code == 204, \
        f'Could not delete theme! Responded with {resp.status_code} and {resp.text}'


def get_theme_id_from_url():
    url = sl.get_location()
    assert '/themes/' in url, 'URL does not contain /themes'
    return url.lstrip(os.getenv('ADMIN_URL')).split('/')[1]


def get_release_guid_from_release_status_page_url(url):
    assert url.endswith('/status')
    url_components = url.split('/')
    return url_components[-2]


def user_triggers_release_on_demand(release_id):
    resp = admin_request('PUT', f'/api/bau/release/{release_id}/publish')
    assert resp.status_code == 200, \
        f'Release on demand request failed! Responded with {resp.status_code} and {resp.text}'


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
            sl.reload_page()
            time.sleep(10)
    raise_assertion_error(f'Release process status wasn\'t {status} after {timeout} seconds!')
