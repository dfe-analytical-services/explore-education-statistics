import os
import time

from robot.libraries.BuiltIn import BuiltIn
from selenium.common import NoSuchElementException
from selenium.webdriver.common.by import By
from tests.libs.logger import get_logger
from tests.libs.setup_auth_variables import setup_auth_variables
from tests.libs.utilities import set_cookie_from_json, set_to_local_storage

sl = BuiltIn().get_library_instance("SeleniumLibrary")

logger = get_logger(__name__)


def raise_assertion_error(err_msg):
    sl.failure_occurred()
    raise AssertionError(err_msg)


def user_signs_in_as(user: str):
    try:
        (local_storage_token, cookie_token) = setup_auth_variables(
            user,
            email=os.getenv(f"{user}_EMAIL"),
            password=os.getenv(f"{user}_PASSWORD"),
            driver=sl.driver,
            identity_provider=os.getenv("IDENTITY_PROVIDER"),
        )

        admin_url = os.getenv("ADMIN_URL")
        assert admin_url

        set_to_local_storage(
            f"GovUk.Education.ExploreEducationStatistics.Adminuser:{admin_url}:GovUk.Education"
            f".ExploreEducationStatistics.Admin",
            local_storage_token,
        )
        set_cookie_from_json(cookie_token)

        sl.go_to(admin_url)
    except Exception as e:
        raise_assertion_error(e)


def get_theme_id_from_url():
    url = sl.get_location()
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
        elem = sl.driver.find_element(By.XPATH, f'//*[@id="dataFileUploadForm"]/dl[{num}]')
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num}"')
    try:
        elem.find_element(By.XPATH, xpath)
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num} with xpath {xpath}')


def data_file_number_contains_xpath(num, xpath):
    try:
        elem = sl.driver.find_element(By.XPATH, f'//*[@id="fileUploadForm"]/dl[{num}]')
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
            sl.driver.find_element(By.ID, f"release-process-status-Failed")
            raise_assertion_error("Release process status FAILED!")
        except BaseException:
            pass
        try:
            sl.driver.find_element(By.ID, f"release-process-status-{status}")
            return
        except BaseException:
            sl.reload_page()  # Necessary if release previously scheduled
            time.sleep(3)
    raise_assertion_error(f"Release process status wasn't {status} after {timeout} seconds!")


def user_checks_dashboard_theme_topic_dropdowns_exist():
    try:
        sl.driver.find_element(By.CSS_SELECTOR, "#publicationsReleases-themeTopic-themeId")
        sl.driver.find_element(By.CSS_SELECTOR, "#publicationsReleases-themeTopic-topicId")
    except NoSuchElementException:
        return False

    return True
