from selenium.webdriver.remote.webelement import WebElement


def take_screenshot_of_element(element: WebElement, filename: str):
    element.screenshot(f'/tmp/{filename}')
