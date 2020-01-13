from logging import warn
from robot.libraries.BuiltIn import BuiltIn
from selenium.webdriver.common.action_chains  import ActionChains
from selenium.webdriver.common.keys import Keys
from selenium.common.exceptions import NoSuchElementException
sl = BuiltIn().get_library_instance('SeleniumLibrary')
from datetime import datetime
import json

def user_sets_focus_to_element(selector):
  if selector.startswith('css:'):
    selector = selector[4:]
    sl.wait_until_page_contains_element(f'css:{selector}')
    elem = sl.driver.find_element_by_css_selector(selector)
  elif selector.startswith('xpath:'):
    selector = selector[6:]
    sl.wait_until_page_contains_element(f'xpath:{selector}')
    elem = sl.driver.find_element_by_xpath(selector)
  else:
    raise AssertionError('Selector must be either css or xpath!')
  elem.send_keys(Keys.NULL)

def set_cookie_from_json(cookie_json):
  cookie_dict = json.loads(cookie_json)
  sl.driver.add_cookie(cookie_dict)

def get_datetime(strf):
  now = datetime.now()
  return now.strftime(strf)

def user_should_be_at_top_of_page():
  (x, y) = sl.get_window_position()
  if y != 0:
    raise AssertionError(f"Windows position Y is {y} not 0! User should be at the top of the page!")

def user_checks_page_contains_accordion(accordion_heading):
  try:
    sl.wait_until_page_contains_element(f'xpath://*[@class="govuk-accordion__section-button" and text()="{accordion_heading}"]')
    # sl.driver.find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{accordion_heading}"]')
  except:
    raise AssertionError(f"Accordion with heading '{accordion_heading}' not found!'")

def user_checks_accordion_is_in_position(header_starts_with, position):
  try:
    # NOTE(mark): When nth-child won't do, you need to do the unholy equivalent of css .class in xpath...
    elem = sl.driver.find_element_by_xpath(f'(.//*[contains(concat(" ", normalize-space(@class), " "), " govuk-accordion__section ")])[{position}]')
  except:
    raise AssertionError(f"There are less than {position} accordion sections!")

  if not elem.text.strip().startswith(header_starts_with):
    raise AssertionError(f'Accordion in position {position} expected start with text "{header_starts_with}". Actual found text: "{elem.text}"')

def user_verifies_accordion_is_open(section_text):
  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{section_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find accordion with header {section_text}')

  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{section_text}" and @aria-expanded="true"]')
  except NoSuchElementException:
    raise AssertionError(f'Accordion section "{section_text}" should have attribute aria-expanded="true"')

def user_verifies_accordion_is_closed(section_text):
  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{section_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find accordion with header {section_text}')

  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{section_text}" and @aria-expanded="false"]')
  except NoSuchElementException:
    raise AssertionError(f'Accordion section "{section_text}" should have attribute aria-expanded="false"')

def user_opens_accordion_section(exact_section_text):
  try:
    elem = sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find accordion with header {exact_section_text}')

  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}" and @aria-expanded="false"]')
  except NoSuchElementException:
    BuiltIn().log_to_console(f'WARNING: Accordion section "{exact_section_text}" already open!')
    return

  try:
    elem.click()
  except:
    raise AssertionError(f'Cannot click accordion section header {exact_section_text}')

  sl.wait_until_page_contains_element(f'xpath://*[@class="govuk-accordion__section-button" and text()="{exact_section_text}" and @aria-expanded="true"]')

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

def user_checks_accordion_section_contains_text(accordion_section, details_component):
  try:
    sl.driver.find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{accordion_section}"]')
  except:
    raise AssertionError(f'Cannot find accordion section "{accordion_section}"')

  try:
    sl.driver.find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{accordion_section}"]/../../..//*[text()="{details_component}"]')
  except:
    raise AssertionError(f'Details component "{details_component}" not found in accordion section "{accordion_section}"')

def user_opens_details_dropdown(exact_details_text):
  try:
    elem = sl.driver.find_element_by_xpath(f'.//*[@class="govuk-details__summary-text" and text()="{exact_details_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'No such detail component "{exact_details_text}" found')

  elem.click()
  if elem.find_element_by_xpath('..').get_attribute("aria-expanded") == "false":
    raise AssertionError(f'Details component "{exact_details_text}" not expanded!')

def user_closes_details_dropdown(exact_details_text):
  try:
    elem = sl.driver.find_element_by_xpath(f'.//*[@class="govuk-details__summary-text" and text()="{exact_details_text}"]')
  except:
    raise AssertionError(f'Cannot find details component "{exact_details_text}"')
  elem.click()
  if elem.find_element_by_xpath('..').get_attribute("aria-expanded") == "true":
    raise AssertionError(f'Details component "{exact_details_text}" is still expanded!')

def user_selects_radio(radio_label):
  sl.driver.find_element_by_xpath(f'//label[text()="{radio_label}"]').click()

def user_checks_radio_is_checked(radio_label):
  try:
    sl.driver.find_element_by_css_selector(f'input[data-testid="{radio_label}"][checked]')
  except NoSuchElementException:
    raise AssertionError('Unable to find checked radio!')

def user_clicks_checkbox(checkbox_label):
  sl.driver.find_element_by_xpath(f'//label[text()="{checkbox_label}"]').click()

def capture_large_screenshot():
  currentWindow = sl.get_window_size()
  page_height = sl._current_browser().execute_script("return document.documentElement.scrollHeight;")

  page_width = currentWindow[0]
  original_height = currentWindow[1]

  sl.set_window_size(page_width, page_height)
  warn("Capturing a screenshot at URL " + sl.get_location())
  sl.capture_page_screenshot()
  sl.set_window_size(page_width, original_height)

def user_checks_previous_table_tool_step_contains(step, key, value):
  try:
    sl.wait_until_page_contains_element(f'xpath://*[@id="tableTool-steps-step-{step}"]//*[text()="Go to this step"]')
    sl.driver.find_element_by_xpath(f'//*[@id="tableTool-steps-step-{step}"]//*[text()="Go to this step"]')
  except:
    sl.capture_page_screenshot()
    raise AssertionError(f'Previous step wasn\'t found!')

  try:
    sl.driver.find_element_by_xpath(
      f'.//*[@id="tableTool-steps-step-{step}"]//dt[text()="{key}"]/../dd[text()="{value}"]')
  except:
    sl.capture_page_screenshot()
    raise AssertionError(f'Element "#tableTool-steps-step-{step}" containing "{key}" and "{value}" not found!')

def user_selects_start_date(start_date):
  sl.select_from_list_by_label('css:#timePeriodForm-start', start_date)

def user_selects_end_date(end_date):
  sl.select_from_list_by_label('css:#timePeriodForm-end', end_date)

def user_clicks_indicator_checkbox(indicator_label):
  sl.driver.find_element_by_xpath(
    f'//*[@id="filtersForm-indicators"]//label[contains(text(),"{indicator_label}")]').click()

def user_clicks_subheaded_indicator_checkbox(subheading_label, indicator_label):
  sl.driver.find_element_by_xpath(
    f'//*[@id="filtersForm-indicators"]//legend[text()="{subheading_label}"]/..//label[text()="{indicator_label}"]').click()

def user_checks_indicator_checkbox_is_selected(indicator_label):
  sl.checkbox_should_be_selected(
    f'xpath://*[@id="filtersForm-indicators"]//label[contains(text(), "{indicator_label}")]/../input')

def user_checks_subheaded_indicator_checkbox_is_selected(subheading_label, indicator_label):
  sl.checkbox_should_be_selected(
    f'xpath://*[@id="filtersForm-indicators"]//legend[text()="{subheading_label}"]/..//label[text()="{indicator_label}"]/../input')

def user_clicks_category_checkbox(subheading_label, category_label):
  sl.driver.find_element_by_xpath(f'//legend[text()="{subheading_label}"]/..//label[text()="{category_label}"]').click()

def user_checks_category_checkbox_is_selected(subheading_label, category_label):
  sl.checkbox_should_be_selected(f'xpath://legend[text()="{subheading_label}"]/..//label[text()="{category_label}"]/../input')

def user_clicks_select_all_for_category(category_label):
  sl.driver.find_element_by_xpath(f'//legend[text()="{category_label}"]/..//button[contains(text(),"Select")]').click()

def user_checks_results_table_column_heading_contains(row, column, expected):
  elem = sl.driver.find_element_by_xpath(f'//table/thead/tr[{row}]/th[{column}]')
  if expected not in elem.text:
    raise AssertionError(
      f'"{expected}" not found in th tag in results table thead row {row}, column {column}. Found text "{elem.text}".')

def user_checks_results_table_row_heading_contains(row, column, expected):
  elem = sl.driver.find_element_by_xpath(f'//table/tbody/tr[{row}]/th[{column}]')
  if expected not in elem.text:
    raise AssertionError(
      f'"{expected}" not found in th tag in results table tbody row {row}, column {column}. Found text "{elem.text}".')

def user_checks_results_table_cell_contains(row, column, expected):
  elem = sl.driver.find_element_by_xpath(f'//table/tbody/tr[{row}]/td[{column}]')
  if expected not in elem.text:
    raise AssertionError(
      f'"{expected}" not found in td tag in results table tbody row {row}, column {column}. Found text "{elem.text}".')

def user_checks_list_contains_label(list_locator, label):
  labels = sl.get_list_items(list_locator)
  if label not in labels:
    raise AssertionError(f'"{label}" wasn\'t found amongst list items "{labels}" from locator "{list_locator}"')

def user_checks_list_does_not_contain_label(list_locator, label):
  labels = sl.get_list_items(list_locator)
  if label in labels:
    raise AssertionError(f'"{label}" was found amongst list items "{labels}" from locator "{list_locator}"')

def user_checks_selected_list_label(list_locator, label):
  selected_label = sl.get_selected_list_label(list_locator)
  if selected_label != label:
    raise AssertionError(f'Selected label "{selected_label}" didn\'t match label "{label} for list "{list_Locator}"')