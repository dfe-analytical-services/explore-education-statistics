import os
from robot.libraries.BuiltIn import BuiltIn
s2l = BuiltIn().get_library_instance('SeleniumLibrary')

import chromedriver_install as cdi

def install_chromedriver():
  path = cdi.install(file_directory='./lib/', verbose=False, chmod=True, overwrite=False, version=None)
  # BuiltIn().log_to_console('Installed chromedriver to path: %s' % path)

def add_lib_dir_to_path():
  os.environ["PATH"] += os.pathsep + os.getcwd() + os.sep + 'lib'
  # BuiltIn().log_to_console('PATH: ' + os.environ["PATH"])
