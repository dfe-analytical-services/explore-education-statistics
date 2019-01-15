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

def get_url_for_env(env):
  if env == "local":
    return BuiltIn().get_variable_value("${localUrl}")
  elif env == "test":
    return BuiltIn().get_variable_value("${testUrl}")
  elif env == "stage":
    return BuiltIn().get_variable_value("${stageUrl}")
  elif env == "prod":
    return BuiltIn().get_variable_value("${prodUrl}")
  else:
    raise AssertionError("Invalid environment \"" + env + "\"!")
