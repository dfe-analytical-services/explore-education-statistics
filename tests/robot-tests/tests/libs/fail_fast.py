"""
This utility class is responsible for allowing us to control the fail-fast behaviour of Robot Test Suites if one of
their Tests fails.  If the "fail tests suites fast" option is enabled, this file's methods are called from Robot
scripts, firstly to record that a test suite is failing, and then again on subsequent Tests starting to see if they
should continue to run or if they should fail immediately and therefore fail the test suite immediately.
"""

import datetime
import os
import os.path
import threading

import tests.libs.visual as visual
from robot.api import SkipExecution
from robot.libraries.BuiltIn import BuiltIn
from tests.libs.logger import get_logger
from tests.libs.selenium_elements import sl

failing_suites_filename = ".failing_suites"

logger = get_logger(__name__)


def record_test_failure():
    if not current_test_suite_failing_fast():
        record_failing_test_suite()
        visual.capture_screenshot()
        visual.capture_large_screenshot()
        _capture_html()

    if BuiltIn().get_variable_value("${prompt_to_continue_on_failure}") == "1":
        _prompt_to_continue()


def _get_current_test_suite() -> str:
    return BuiltIn().get_variable_value("${SUITE SOURCE}")


def current_test_suite_failing_fast() -> bool:
    test_suite = _get_current_test_suite()
    failing_suites = get_failing_test_suites()
    return f"{test_suite}" in failing_suites


file_lock = threading.Lock()


def record_failing_test_suite():
    if current_test_suite_failing_fast():
        return

    test_suite = _get_current_test_suite()

    logger.info(
        f"Recording test suite '{test_suite}' as failing - subsequent tests will automatically fail in this suite"
    )

    with file_lock:
        try:
            with open(failing_suites_filename, "a") as file_write:
                file_write.write(f"{test_suite}{os.linesep}")
        except IOError as e:
            logger.error(f"Failed to write failing test suite to file: {e}")


def fail_test_fast_if_required():
    if current_test_suite_failing_fast():
        raise SkipExecution("")


def get_failing_test_suites() -> []:
    with file_lock:
        if os.path.isfile(failing_suites_filename):
            try:
                with open(failing_suites_filename, "r") as file:
                    return [suite.strip() for suite in file.readlines()]
            except IOError as e:
                logger.error(f"Failed to read failing test suites from file: {e}")
                return []
        return []


def _capture_html():
    html = sl().get_source()
    current_time_millis = round(datetime.datetime.timestamp(datetime.datetime.now()) * 1000)
    output_dir = BuiltIn().get_variable_value("${OUTPUT DIR}")
    html_file = open(f"{output_dir}{os.sep}captured-html-{current_time_millis}.html", "w", encoding="utf-8")
    html_file.write(html)
    html_file.close()
    logger.info(f"Captured HTML of {sl().get_location()}      HTML saved to file://{os.path.realpath(html_file.name)}")


def _prompt_to_continue():
    logger.warn("Continue? (Y/n)")
    choice = input()
    if choice.lower().startswith("n"):
        _raise_assertion_error("Tests stopped!")


def _raise_assertion_error(err_msg):
    sl().failure_occurred()
    raise AssertionError(err_msg)
