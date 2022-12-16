from robot.api import SuiteVisitor
from tests.libs.logger import get_logger


class CheckForAtLeastOnePassingRunPrerebotModifier(SuiteVisitor):
    logger = get_logger(__name__)

    def __init__(self, *args):
        pass

    def visit_test(self, test):
        if "Test has been re-executed and results merged." in test.message:
            if "PASS" in test.message:
                self.logger.info(
                    f'CheckForAtLeastOnePassingRunPrerebotModifier - marking test "{test}" as PASS because it passed in at least one of the test runs.'
                )
                test.status = "PASS"
                test.message = f'Marking test "{test}" as PASS because it passed in at least one of the test runs.  Previous message is {test.message}'
            elif "SKIP" in test.message:
                self.logger.info(
                    f'CheckForAtLeastOnePassingRunPrerebotModifier - marking test "{test}" as SKIPPED because it was skipped in the latter run.'
                )
                test.status = "SKIP"
                test.message = f'Marking test "{test}" as SKIP because it was skipped in the latter test run.  Previous message is {test.message}'
