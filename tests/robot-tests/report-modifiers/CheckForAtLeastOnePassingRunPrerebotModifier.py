from robot.api import SuiteVisitor
from logging import info


class CheckForAtLeastOnePassingRunPrerebotModifier(SuiteVisitor):

    def __init__(self, *args):
        pass

    def visit_test(self, test):
        if 'PASS' in test.message and 'Test has been re-executed and results merged.' in test.message:
            info(
                f"CheckForAtLeastOnePassingRunPrerebotModifier - marking test \"{test}\" as PASS because it passed in at least one of the test runs.")
            test.status = 'PASS'
            test.message = f'Marking test \"{test}\" as PASS because it passed in at least one of the test runs.  Previous message is {test.message}'
