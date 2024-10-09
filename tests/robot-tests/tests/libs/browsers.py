import selenium.webdriver as webdriver
from robot.libraries.BuiltIn import BuiltIn
from tests.libs.selenium_elements import sl

def create_chrome_headless(alias: str):
    chrome_options = webdriver.ChromeOptions()
    chrome_options.add_argument("headless=chrome")
    chrome_options.add_argument("start-maximized")
    chrome_options.add_argument("disable-extensions")
    chrome_options.add_argument("disable-infobars")
    chrome_options.add_argument("disable-gpu")
    chrome_options.add_argument("window-size=1920,1080")
    chrome_options.add_argument("no-first-run")
    chrome_options.add_argument("no-default-browser-check")
    chrome_options.add_argument("ignore-certificate-errors")
    chrome_options.add_argument("log-level=3")
    chrome_options.add_argument("disable-logging")

    chrome_options.add_experimental_option("prefs", {
        "download.default_directory": BuiltIn().get_variable_value('${DOWNLOADS_DIR}'),
    })
    
    driver = sl().create_webdriver(driver_name='Chrome', alias=alias, options=chrome_options)
    sl().driver.execute_cdp_cmd('Network.setBlockedURLs', {"urls": ["*woff*", "*png", "*svg"]})
    sl().driver.execute_cdp_cmd('Network.enable', {})
