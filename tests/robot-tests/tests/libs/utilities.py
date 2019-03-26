from robot.libraries.BuiltIn import BuiltIn
from selenium.webdriver.common.by import By
s2l = BuiltIn().get_library_instance('SeleniumLibrary')

def cookie_names_should_be_on_page():
  cookies = s2l.driver.get_cookies()
  for cookie in cookies:
    s2l.page_should_contain(cookie['name'])

def get_matching_css_count(css):
  if css.startswith('css:'):
    css = css[4:]
  return len( s2l._current_browser().find_elements(By.CSS_SELECTOR, css) )

def css_should_match_x_times(css, times):
  if css.startswith('css:'):
    css = css[4:]
  elements = s2l._current_browser().find_elements(By.CSS_SELECTOR, css)
  if len(elements) != int(times):
    raise AssertionError("\"CSS Should Match X Times\" found " + str(len(elements)) + " matching elements, not " + str(times) + " elements!")
