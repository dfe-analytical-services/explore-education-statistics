from tests.libs.logger import get_logger
from tests.libs.selenium_elements import sl

logger = get_logger(__name__)


def get_browser_timezone():
    return sl().driver.execute_script('return Intl.DateTimeFormat().resolvedOptions().timeZone;')


def get_browser_utc_offset():
    return sl().driver.execute_script('return new Date().getTimezoneOffset();')