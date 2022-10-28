"""
This utility class is responsible for allowing us to control the fail-fast behaviour of Robot Test Suites if one of
their Tests fails.  If the "fail tests suites fast" option is enabled, this file's methods are called from Robot
scripts, firstly to record that a test suite is failing, and then again on subsequent Tests starting to see if they
should continue to run or if they should fail immediately and therefore fail the test suite immediately.
"""
from robot.libraries.BuiltIn import BuiltIn
from tests.libs.logger import get_logger

sl = BuiltIn().get_library_instance("SeleniumLibrary")
FAILING_SUITES = set()
logger = get_logger(__name__)


def current_test_suite_failing_fast() -> bool:
    test_suite = _get_current_test_suite()
    return test_suite in FAILING_SUITES


def record_failing_test_suite():
    test_suite = _get_current_test_suite()
    logger.warn(
        f"Recording test suite '{test_suite}' as failing - subsequent tests will automatically fail in this suite"
    )
    FAILING_SUITES.add(test_suite)


def fail_test_fast_if_required():
    if current_test_suite_failing_fast():
        _raise_assertion_error(f"Test suite {_get_current_test_suite()} is already failing.  Failing this test fast.")


def _get_current_test_suite() -> str:
    return BuiltIn().get_variable_value("${SUITE SOURCE}")


def _raise_assertion_error(err_msg):
    sl.failure_occurred()
    raise AssertionError(err_msg)
