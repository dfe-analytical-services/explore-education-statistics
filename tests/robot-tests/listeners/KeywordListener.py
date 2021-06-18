import os.path
import tempfile
from logging import warn

class KeywordListener:
    ROBOT_LISTENER_API_VERSION = 2
    
#     def start_suite(self, suite, result):
#         suite.tests.create(name='New test')
#     
#     def start_test(self, test, result):
#         warn('Test started')
#         
    def start_keyword(self, name, attributes):
        print(f'\t{attributes["kwname"]}   ${attributes["args"]}')