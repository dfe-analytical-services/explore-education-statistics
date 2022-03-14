from selenium.webdriver.remote.webelement import WebElement
from os import getcwd, path, pardir, makedirs
from robot.libraries.BuiltIn import BuiltIn
from logging import warning

sl = BuiltIn().get_library_instance('SeleniumLibrary')


def with_maximised_browser(func):
    def wrapper(*args, **kwargs):
        currentWindow = sl.get_window_size()
        page_height = sl.driver.execute_script(
            "return document.documentElement.scrollHeight;")

        page_width = currentWindow[0]
        original_height = currentWindow[1]

        sl.set_window_size(page_width, page_height)

        try:
            return func(*args, **kwargs)
        finally:
            sl.set_window_size(page_width, original_height)
    return wrapper


def highlight_element(element: WebElement):
    sl.driver.execute_script(
        "arguments[0].scrollIntoView();", element)
    sl.driver.execute_script(
        "arguments[0].style.border = 'red 4px solid';", element)


@with_maximised_browser
def capture_large_screenshot():
    screenshot_location = sl.capture_page_screenshot()
    warning(f"Captured a screenshot at URL {sl.get_location()}     Screenshot saved to file://{screenshot_location}")


@with_maximised_browser
def take_screenshot_of_element(element: WebElement, filename: str):
    filepath = f'{getcwd()}/{filename}'
    folder = path.abspath(path.join(filepath, pardir))
    makedirs(folder, exist_ok=True)
    element.screenshot(filepath)
    return f'file://{filepath}'
