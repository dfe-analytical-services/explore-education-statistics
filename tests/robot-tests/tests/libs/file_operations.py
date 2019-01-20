from robot.libraries.BuiltIn import BuiltIn
s2l = BuiltIn().get_library_instance('SeleniumLibrary')
import os
import requests
import zipfile

def download_file(url, name):
  if not os.path.exists('test-results/downloads'):
    os.makedirs('test-results/downloads')
  r = requests.get(url, allow_redirects=True)
  open('test-results/downloads/' + name, 'wb').write(r.content)

def zip_should_contain_file(zipfilename, filename):
  zip = zipfile.ZipFile('test-results/downloads/' + zipfilename)
  files_in_zip = zip.namelist()
  if filename not in files_in_zip:
    raise AssertionError('File \"' + filename + '\" not found in \"' + zipfilename + '\", which contains ' + str(files_in_zip))
