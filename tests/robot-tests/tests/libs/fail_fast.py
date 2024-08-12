"""
This utility class is responsible for allowing us to control the fail-fast behaviour of Robot Test Suites if one of
their Tests fails.  If the "fail tests suites fast" option is enabled, this file's methods are called from Robot
scripts, firstly to record that a test suite is failing, and then again on subsequent Tests starting to see if they
should continue to run or if they should fail immediately and therefore fail the test suite immediately.
"""

import os.path
import os
import threading

from robot.libraries.BuiltIn import BuiltIn
from tests.libs.logger import get_logger
from tests.libs.selenium_elements import sl

failing_suites_filename = ".failing_suites"

logger = get_logger(__name__)


def _get_current_test_suite() -> str:
    return BuiltIn().get_variable_value("${SUITE SOURCE}")


def current_test_suite_failing_fast() -> bool:
    test_suite = _get_current_test_suite()
    failing_suites = get_failing_test_suites()
    return f"{test_suite}" in failing_suites


file_lock = threading.Lock()


def record_failing_test_suite():
    test_suite = _get_current_test_suite()
    logger.warn(
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
        _raise_assertion_error(f"Test suite {_get_current_test_suite()} is already failing.  Failing this test fast.")


def get_failing_test_suites() -> []:
    if os.path.isfile(failing_suites_filename):
        # We wouldn't expect the same test suite to be recorded in this file more than once, as we only trigger the
        # "record failing test suite" upon the first failing test in an individual suite.
        #
        # Strangely though, this does get called multiple times if using Pabot and re-running failed suites. It seems as
        # though the failure keywords are being merged with the initial run's failure keyword definitions and therefore
        # causing the failing test suite to be recorded multiple times when its first failing test is hit.
        #
        # We therefore explicitly remove any duplicates from the list here.

        if os.path.isfile(failing_suites_filename):
            try:
                with open(failing_suites_filename, "r") as file:
                    failing_suites = file.readlines()
                    stripped_suite_names = [failing_suite.strip() for failing_suite in failing_suites]
                    filtered_suite_names = filter(None, stripped_suite_names)
                    return list(dict.fromkeys(filtered_suite_names))
            except IOError as e:
                logger.error(f"Failed to read failing test suites from file: {e}")
                return []
        return []


def _raise_assertion_error(err_msg):
    sl().failure_occurred()
    raise AssertionError(err_msg)
