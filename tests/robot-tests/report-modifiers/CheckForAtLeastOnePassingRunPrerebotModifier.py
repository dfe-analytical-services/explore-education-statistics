from robot.api import SuiteVisitor
from logging import info

class CheckForAtLeastOnePassingRunPrerebotModifier(SuiteVisitor):

    def __init__(self, *args):
        pass

    def visit_test(self, test):
        if test.status == 'PASS' and 'Test has been re-executed and results merged.' in test.message:
            info(f"CheckForAtLeastOnePassingRunPrerebotModifier - marking test \"{test}\" as PASS because it passed in at least one of the test runs.")
            test.status = 'PASS'
            test.message = 'Test passed because it passed at least once.'