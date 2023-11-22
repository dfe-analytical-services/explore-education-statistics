from tests.libs.logger import get_logger
from tests.libs.fail_fast import remove_failing_suite
import warnings
import pprint

class SuiteEndedListener:
    ROBOT_LISTENER_API_VERSION = 2

    logger = get_logger(__name__)

    invoked = 0

    def start_suite(self, test_suite, attributes):
        self.logger.warn("STARTING SUITE!!!!!!!!!!!!!!!!!!!!!!!!!!!")
        remove_failing_suite()