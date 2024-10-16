import os
import time

import requests
from selenium.common import NoSuchElementException
from selenium.webdriver.common.by import By
from tests.libs import local_storage_helper
from tests.libs.logger import get_logger
from tests.libs.selenium_elements import sl
from tests.libs.setup_auth_variables import setup_auth_variables

logger = get_logger(__name__)


def raise_assertion_error(err_msg):
    sl().failure_occurred()
    logger.warning(err_msg)
    raise AssertionError(err_msg)


class PublisherFunctionsClient:
    ROBOT_AUTO_KEYWORDS = False

    def __init__(self):
        assert (
            os.getenv("PUBLISHER_FUNCTIONS_URL") is not None
        ), "Environment variable PUBLISHER_FUNCTIONS_URL must be provided"

        requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
        self.session = requests.Session()
        self.base_url = os.getenv("PUBLISHER_FUNCTIONS_URL")

    @staticmethod
    def __request(self, method: str, url: str, body: object = None):
        assert method and url, f"Method and URL must be provided, got method {method} and url {url}"
        headers = {
            "Content-Type": "application/json",
        }
        return self.session.request(
            method, url=f"{self.base_url}{url}", headers=headers, stream=True, json=body, verify=False
        )

    def post(self, url: str, body: object = None):
        return self.__request(self, "POST", url, body)


publisher_functions_client = PublisherFunctionsClient()


def user_signs_in_as(user: str):
    admin_url = os.getenv("ADMIN_URL")
    assert admin_url, "ADMIN_URL env variable must be set"

    try:
        local_storage_json = setup_auth_variables(
            user,
            email=os.getenv(f"{user}_EMAIL"),
            password=os.getenv(f"{user}_PASSWORD"),
            driver=sl().driver,
            identity_provider=os.getenv("IDENTITY_PROVIDER"),
        )

        local_storage_helper.write_local_storage_json_to_local_storage(local_storage_json, sl().driver)

        sl().go_to(admin_url)
    except Exception as e:
        raise_assertion_error(e)


def get_theme_id_from_url():
    url = sl().get_location()
    assert "/themes/" in url, "URL does not contain /themes"
    result = url[len(os.getenv("ADMIN_URL")) :].lstrip("/").split("/")
    assert result[0] == "themes", 'String "themes" should be 1st element in list'
    return result[1]


def get_release_guid_from_release_status_page_url(url):
    assert url.endswith("/status")
    url_components = url.split("/")
    return url_components[-2]


def data_csv_number_contains_xpath(num, xpath):
    try:
        elem = sl().driver.find_element(By.XPATH, f'//*[@id="dataFileUploadForm"]/dl[{num}]')
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num}"')
    try:
        elem.find_element(By.XPATH, xpath)
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num} with xpath {xpath}')


def data_file_number_contains_xpath(num, xpath):
    try:
        elem = sl().driver.find_element(By.XPATH, f'//*[@id="fileUploadForm"]/dl[{num}]')
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num}"')
    try:
        elem.find_element(By.XPATH, xpath)
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num} with xpath {xpath}')


def user_waits_for_release_process_status_to_be(status, timeout):
    max_time = time.time() + int(timeout)
    while time.time() < max_time:
        try:
            sl().driver.find_element(By.ID, f"release-process-status-Failed")
            raise_assertion_error("Release process status FAILED!")
        except BaseException:
            pass
        try:
            sl().driver.find_element(By.ID, f"release-process-status-{status}")
            return
        except BaseException:
            sl().reload_page()  # Necessary if release previously scheduled
            time.sleep(3)
    raise_assertion_error(f"Release process status wasn't {status} after {timeout} seconds!")


def user_checks_dashboard_theme_dropdown_exists():
    try:
        sl().driver.find_element(By.ID, "publicationsReleases-theme")
    except NoSuchElementException:
        return False

    return True


def trigger_immediate_staging_of_scheduled_release(release_version_id):
    stage_release_version_response = publisher_functions_client.post(
        "/api/StageScheduledReleaseVersionsImmediately", {"releaseVersionIds": [release_version_id]}
    )
    assert (
        stage_release_version_response.status_code < 300
    ), f"Immediate staging of scheduled release version API request failed with {stage_release_version_response.status_code} and {stage_release_version_response.text}"


def trigger_immediate_publishing_of_scheduled_release(release_version_id):
    stage_release_version_response = publisher_functions_client.post(
        "/api/PublishStagedReleaseVersionContentImmediately", {"releaseVersionIds": [release_version_id]}
    )
    assert (
        stage_release_version_response.status_code < 300
    ), f"Immediate publishing of staged release version API request failed with {stage_release_version_response.status_code} and {stage_release_version_response.text}"
