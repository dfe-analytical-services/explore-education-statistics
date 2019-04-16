from robot.libraries.BuiltIn import BuiltIn
from selenium.webdriver.common.action_chains  import ActionChains
from selenium.webdriver.common.keys  import Keys
from selenium.webdriver.common.by import By
s2l = BuiltIn().get_library_instance('SeleniumLibrary')

def cookie_names_should_be_on_page():
  cookies = s2l.driver.get_cookies()
  for cookie in cookies:
    s2l.page_should_contain(cookie['name'])

def user_should_be_at_top_of_page():
  (x, y) = s2l.get_window_position()
  if y != 0:
    raise AssertionError("Windows position Y is %s not 0! User should be at the top of the page!" % y)

def get_matching_css_count(css):
  if css.startswith('css:'):
    css = css[4:]
  return len( s2l._current_browser().find_elements(By.CSS_SELECTOR, css) )

def css_should_match_x_times(css, times):
  if css.startswith('css:'):
    css = css[4:]
  elements = s2l._current_browser().find_elements(By.CSS_SELECTOR, css)
  if len(elements) != int(times):
    raise AssertionError("\"CSS Should Match X Times\" found %s matching elements, not %s elements!" % (str(len(elements)), times))

def elements_containing_text_should_match_x_times(text, times):
  elements = s2l._current_browser().find_elements_by_xpath('//*[contains(text(), "%s")]' % text)
  if len(elements) != int(times):
    raise AssertionError("\"Element Containing Text Should Match X Times\" found %s matching elements, not %s elements!" % (str(len(elements)), times))


def user_deletes_text_from_element_until_block_is_empty(selector, text=None):
  element = None
  if selector.startswith('xpath:'):
    selector = selector[6:]
    element = s2l._current_browser().find_element_by_xpath(selector)
  elif selector.startswith('css:'):
    selector = selector[4:]
    element = s2l._current_browser().find_element_by_css_selector(selector)

  if text:
    element = element.find_element_by_xpath('..//*[contains(text(), "%s")]' % text)

  while element.text:
    ActionChains(s2l.driver).send_keys(Keys.DELETE).perform()

def user_clicks_element_containing_text(text):
  s2l._current_browser().find_element_by_xpath('//*[contains(text(), "%s")]' % text).click()

def user_clicks_element_child_containing_text(selector, text):
  element = None
  if selector.startswith('xpath:'):
    selector = selector[6:]
    element = s2l._current_browser().find_element_by_xpath(selector)
  elif selector.startswith('css:'):
    selector = selector[4:]
    element = s2l._current_browser().find_element_by_css_selector(selector)

  element.find_element_by_xpath('.//*[contains(text(), "%s")]' % text).click()

def user_drags_and_drops(drag_selector, drop_selector):
  drag_elem = None
  drop_elem = None
  if drag_selector.startswith('css:'):
    drag_selector = drag_selector[4:]
    drag_elem = s2l._current_browser().find_element_by_css_selector(drag_selector)
  if drop_selector.startswith('css:'):
    drop_selector = drop_selector[4:]
    drop_elem = s2l._current_browser().find_element_by_css_selector(drop_selector)

  # https://github.com/react-dnd/react-dnd/issues/1195#issuecomment-456370983
  action = ActionChains(s2l.driver)
  action.click_and_hold(drag_elem).perform()
  action.move_to_element(drop_elem).perform()
  action.move_by_offset(0, 0).pause(0.01).perform()
  action.release().perform()

def italic_x_characters_before_cursor(num):
  action = ActionChains(s2l.driver).key_down(Keys.SHIFT).send_keys(Keys.ARROW_LEFT).key_up(Keys.SHIFT)
  for x in range(0, int(num)):
    action.perform()

  s2l._current_browser().find_element_by_css_selector('.ck-button:nth-child(4)').click()

  action = ActionChains(s2l.driver).send_keys(Keys.ARROW_RIGHT)
  for x in range(0, int(num)):
    action.perform()

def insert_image():
  s2l._current_browser().find_element_by_css_selector('span.ck-file-dialog-button:nth-of-type(2)').click()
  s2l._current_browser().send_keys("Screenshot")
