from selenium.webdriver.remote.webelement import WebElement
from os import getcwd


def take_screenshot_of_element(element: WebElement, filename: str):
    filepath = f'{getcwd()}/test-results/{filename}'
    element.screenshot(filepath)
    return f'file://{filepath}'
