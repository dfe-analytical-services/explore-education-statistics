import time
from robot.libraries.BuiltIn import BuiltIn
from selenium.webdriver.common.action_chains  import ActionChains
from selenium.webdriver.common.keys  import Keys
from selenium.webdriver.common.by import By
from selenium.common.exceptions import NoSuchElementException
sl = BuiltIn().get_library_instance('SeleniumLibrary')

def cookie_names_should_be_on_page():
  cookies = sl.driver.get_cookies()
  for cookie in cookies:
    if cookie['name'] == '_hjIncludedInSample':
      continue
    sl.page_should_contain(cookie['name'])

def user_should_be_at_top_of_page():
  (x, y) = sl.get_window_position()
  if y != 0:
    raise AssertionError(f"Windows position Y is {y} not 0! User should be at the top of the page!")

# def italic_x_characters_before_cursor(num):
#   action = ActionChains(sl.driver).key_down(Keys.SHIFT).send_keys(Keys.ARROW_LEFT).key_up(Keys.SHIFT)
#   for x in range(0, int(num)):
#     action.perform()
#
#   sl.driver.find_element_by_css_selector('.ck-button:nth-child(4)').click()
#
#   action = ActionChains(sl.driver).send_keys(Keys.ARROW_RIGHT)
#   for x in range(0, int(num)):
#     action.perform()

# def insert_image():
#   sl.driver.find_element_by_css_selector('span.ck-file-dialog-button:nth-of-type(2)').click()
#   sl.driver.send_keys("Screenshot")

def user_checks_accordion_is_in_position(header_starts_with, position):
  try:
    elem = sl.driver.find_element_by_xpath(f'(.//*[@class="govuk-accordion__section"])[{position}]')
  except:
    raise AssertionError(f"There are less than {position} accordion sections!")

  if not elem.text.startswith(header_starts_with):
    raise AssertionError(f'Accordion in position {position} expected start with text "{header_starts_with}". Actual found text: "{elem.text}"')

def user_opens_accordion_section(exact_section_text):
  try:
    sl.driver\
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}"]')\
      .click()
  except NoSuchElementException:
    raise AssertionError(f'Cannot find accordion with header {exact_section_text}')

  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}" and @aria-expanded="true"]')
  except NoSuchElementException:
    raise AssertionError(f'Accordion "{exact_section_text} not expanded!')

def user_closes_accordion_section(exact_section_text):
  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}"]') \
      .click()
  except NoSuchElementException:
    raise AssertionError(f'Cannot find accordion with header {exact_section_text}')

  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}" and @aria-expanded="false"]')
  except NoSuchElementException:
    raise AssertionError(f'Accordion "{exact_section_text}" not collapsed!')

def user_opens_details_dropdown(exact_details_text):
  try:
    elem = sl.driver.find_element_by_xpath(f'.//*[@class="govuk-details__summary-text" and text()="{exact_details_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'No such detail component "{exact_details_text}" found')

  elem.click()
  if elem.find_element_by_xpath('..').get_attribute("aria-expanded") == "false":
    raise AssertionError(f'Details component "{exact_details_text}" not expanded!')

def user_closes_details_dropdown(exact_details_text):
  elem = sl.driver.find_element_by_xpath(f'.//*[@class="govuk-details__summary-text" and text()="{exact_details_text}"]')
  elem.click()
  if elem.find_element_by_xpath('..').get_attribute("aria-expanded") == "true":
    raise AssertionError(f'Details component "{exact_details_text}" is still expanded!')

def user_checks_key_stat_tile_contents(tile_title, tile_value, tile_context):
  try:
    elem = sl.driver.find_element_by_xpath(f'.//*[@data-testid="key-stat-tile-title" and contains(text(), "{tile_title}")]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find key stats tile "{tile_title}"')

  try:
    elem.find_element_by_xpath(f'../p[text()="{tile_value}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find key stat tile "{tile_title}" with value "{tile_value}"')

  try:
    elem.find_element_by_xpath(f'../p[text()="{tile_context}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find key stat tile "{tile_title}" with context "{tile_context}"')

def user_checks_key_stat_bullet_exists(bullet_text):
  elem = sl.driver.find_element_by_xpath('.//*[@id="datablock_keystats_summary"]')
  try:
    elem.find_element_by_xpath(f'.//li[text()="{bullet_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find KeyStat summary bullet "{bullet_text}"')

def user_checks_number_of_previous_releases_is_correct(number):
  elems = sl.driver.find_elements_by_xpath('(.//*[@data-testid="previous-release-item"])')
  if len(elems) != int(number):
    raise AssertionError(f'Found "{len(elems)}" previous releases, not "{int(number)}"')

def user_checks_previous_release_is_shown(release_name):
  try:
    sl.driver.find_element_by_xpath(f'.//*[@data-testid="previous-release-item" and a/text()="{release_name}"]')
  except:
    raise AssertionError(f'No previous release "{release_name}" found')

def user_checks_previous_release_is_shown_in_position(release_name, position):
  try:
    sl.driver.find_element_by_xpath(f'.//*[@data-testid="previous-release-item" and a/text()="{release_name}"]')
  except:
    raise AssertionError(f'No previous release "{release_name}" found')

  try:
    elem = sl.driver.find_element_by_xpath(f'(.//a[../@data-testid="previous-release-item"])[{position}]')
  except:
    raise AssertionError(f"There are less than {position} previous releases listed!")

  if release_name != elem.text:
    raise AssertionError(f'Previous release "{release_name}" not in position {position}. Found "{elem.text}" instead!')

def user_checks_number_of_updates_is_correct(number):
  elems = sl.driver.find_elements_by_xpath('(.//*[@data-testid="last-updated-element"])')
  if len(elems) != int(number):
    raise AssertionError(f'Found "{len(elems)}" updates, not "{int(number)}"')

def user_checks_update_exists(date, text_starts_with):
  try:
    elem = sl.driver.find_element_by_xpath(f'.//*[@data-testid="last-updated-element" and time/text()="{date}"]')
  except NoSuchElementException:
    raise AssertionError(f'No update with date "{date}" found')

  try:
    elem.find_element_by_xpath(f'./p[starts-with(text(), "{text_starts_with}")]')
  except NoSuchElementException:
    raise AssertionError(f'No update on "{date}" found starting with text "{text_starts_with}"')

def user_checks_previous_table_tool_step_contains(css, key, value):
  if css.startswith('css:'):
    css = css[4:]
  try:
    elem = sl.driver.find_element_by_css_selector(css)
  except:
    raise AssertionError(f'Element "{css}" not found!')

  try:
    elem.find_element_by_xpath(f'.//dt[text()="{key}"]')
  except:
    raise AssertionError(f'Element "{css}" containing "{key}" not found!')

  try:
    elem.find_element_by_xpath(f'.//dd[text()="{value}"]')
  except:
    raise AssertionError(f'Element "{css}" containing "{value}" not found!')

def user_reorders_table_headers(drag_selector, drop_selector):
  drag_elem = None
  drop_elem = None
  if drag_selector.startswith('css:'):
    drag_selector = drag_selector[4:]
    drag_elem = sl.driver.find_element_by_css_selector(drag_selector)
  if drop_selector.startswith('css:'):
    drop_selector = drop_selector[4:]
    drop_elem = sl.driver.find_element_by_css_selector(drop_selector)

  # https://github.com/react-dnd/react-dnd/issues/1195#issuecomment-456370983
  action = ActionChains(sl.driver)
  action.click_and_hold(drag_elem).perform()
  action.move_to_element(drop_elem).perform()
  action.move_by_offset(0, 0).pause(0.01).perform()
  action.release().perform()

