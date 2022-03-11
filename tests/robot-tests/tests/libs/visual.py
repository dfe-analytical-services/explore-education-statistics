from selenium.webdriver.remote.webelement import WebElement
from os import getcwd
from robot.libraries.BuiltIn import BuiltIn

sl = BuiltIn().get_library_instance('SeleniumLibrary')


def highlight_element(element: WebElement):
    sl.driver.execute_script(
        "arguments[0].scrollIntoView();", element)
    sl.driver.execute_script(
        "arguments[0].style.border = 'red 4px solid';", element)


def take_screenshot_of_element(element: WebElement, filename: str):
    filepath = f'{getcwd()}/test-results/{filename}'
    element.screenshot(filepath)
    return f'file://{filepath}'
