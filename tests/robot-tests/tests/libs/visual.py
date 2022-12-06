import os

from robot.libraries.BuiltIn import BuiltIn
from selenium.webdriver.remote.webelement import WebElement
from tests.libs.logger import get_logger

sl = BuiltIn().get_library_instance("SeleniumLibrary")
logger = get_logger(__name__)


def with_no_overflow(func):
    def wrapper(*args, **kwargs):
        head_html = sl.driver.execute_script("return document.head.innerHTML;")
        sl.driver.execute_script("document.head.innerHTML += '<style>* {overflow: unset !important}</style>'")

        try:
            return func(*args, **kwargs)
        finally:
            sl.driver.execute_script("document.head.innerHTML = arguments[0];", head_html)

    return wrapper


def with_maximised_browser(func):
    def wrapper(*args, **kwargs):
        currentWindow = sl.get_window_size()
        page_width = sl.driver.execute_script("return document.documentElement.scrollWidth;") + 100
        page_height = sl.driver.execute_script("return document.documentElement.scrollHeight;") + 100

        original_width = currentWindow[0]
        original_height = currentWindow[1]

        sl.set_window_size(page_width, page_height)

        try:
            return func(*args, **kwargs)
        finally:
            sl.set_window_size(original_width, original_height)

    return wrapper


def highlight_element(element: WebElement):
    sl.driver.execute_script("arguments[0].scrollIntoView();", element)
    sl.driver.execute_script("arguments[0].style.border = 'red 4px solid';", element)


@with_maximised_browser
def capture_large_screenshot():
    screenshot_location = sl.capture_page_screenshot()
    logger.warn(f"Captured a screenshot at URL '{sl.get_location()}' Screenshot saved to file://{screenshot_location}")


@with_no_overflow
@with_maximised_browser
def take_screenshot_of_element(element: WebElement, filename: str):
    try:
        filepath = f"{os.getcwd()}{os.sep}{filename}"
        folder = os.path.abspath(os.path.join(filepath, os.pardir))
        os.makedirs(folder, exist_ok=True)
        element.screenshot(filepath)
        return f"file://{filepath}"
    except BaseException:
        logger.warn(f"Unable to take a screenshot of element for file {filename}")
        return ""


def take_html_snapshot_of_element(element: WebElement, filename: str):
    filepath = f"{os.getcwd()}{os.sep}{filename}"
    folder = os.path.abspath(os.path.join(filepath, os.pardir))
    os.makedirs(folder, exist_ok=True)
    html = element.get_attribute("innerHTML")
    with open(filepath, "w", encoding="utf-8") as html_file:
        html_file.write(html)
        html_file.close()
    return f"file://{filepath}"
