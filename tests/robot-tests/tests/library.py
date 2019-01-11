import os
#log_to_console = BuiltIn().log_to_console
from robot.libraries.BuiltIn import BuiltIn
from selenium.webdriver.common.by import By
s2l = BuiltIn().get_library_instance('SeleniumLibrary')

import chromedriver_install as cdi

def install_chromedriver():
  path = cdi.install(file_directory='./lib/', verbose=True, chmod=True, overwrite=True, version=None)
  BuiltIn().log_to_console('Installed chromedriver to path: %s' % path)

def add_lib_dir_to_path():
  os.environ["PATH"] += os.pathsep + os.getcwd() + os.sep + 'lib'
  BuiltIn().log_to_console(os.environ["PATH"])

def get_matching_css_count(css):
  if css.startswith('css:'):
    css = css[4:]
  return len( s2l._current_browser().find_elements(By.CSS_SELECTOR, css) )

def css_should_match_x_times(css, times):
  if css.startswith('css:'):
    css = css[4:]
  elements = s2l._current_browser().find_elements(By.CSS_SELECTOR, css)
  if len(elements) != int(times):
    raise AssertionError("\"CSS Should Match X Times\" found " + str(len(elements)) + " matching elements, not " + str(times) + " elements!")

def import_jquery(url):
  if s2l._current_browser().execute_script("return typeof(jQuery) == 'undefined'"):
    jscode = '''var headNode = document.getElementsByTagName('head')[0]; 
                var scriptNode = document.createElement('script'); 
                scriptNode.type='text/javascript'; 
                scriptNode.src='%s'; 
                headNode.appendChild(scriptNode);''' % url

    s2l._current_browser().execute_script(jscode)

def get_matching_jquery_count(jquery):
  jscode = '''
  var count=0;
  jQuery("%s").each(function() {
    count++;
  });
  return count;
  ''' % jquery.replace("'", "\\'").replace("\"", "\\'")

  return s2l._current_browser().execute_script(jscode)

def jquery_should_match_x_times(jquery, times):
  jscode = '''
  var count=0;
  jQuery("%s").each(function() {
    count++;
  });
  return count;
  ''' % jquery.replace("'", "\\'").replace("\"", "\\'")

  num = int(s2l._current_browser().execute_script(jscode))
  if num != int(times):
    raise AssertionError("\"jQuery Should Match X Times\" found " + str(num) + " matching elements, not " + str(times) + " elements!")
