from robot.libraries.BuiltIn import BuiltIn
sl = BuiltIn().get_library_instance('SeleniumLibrary')
import os
import requests
import zipfile

def download_file(link_locator, file_name):
  if not os.path.exists('test-results/downloads'):
    os.makedirs('test-results/downloads')
  link_url = sl.get_element_attribute (link_locator, 'href')
  r = requests.get(link_url, allow_redirects=True)
  f = open(f'test-results/downloads/{file_name}', 'wb')
  f.write(r.content)
  f.close()

def downloaded_file_should_have_first_line(filename, expected_first_line):
  f = open(f'test-results/downloads/{filename}', 'r')
  first_line = f.readline().rstrip()
  f.close()
  if first_line != expected_first_line:
    raise AssertionError(f'First line of file "{filename}" didn\'t match expected first line: "{expected_first_line}"')

def zip_should_contain_file(zipfilename, filename):
  zip = zipfile.ZipFile(f'test-results/downloads/${zipfilename}')
  files_in_zip = zip.namelist()
  if filename not in files_in_zip:
    raise AssertionError(f'File "{filename}" not found in "{zipfilename}", which contains {str(files_in_zip)}')

if __name__ == '__main__':
  download_file('https://s101d01-as-ees-data.azurewebsites.net/api/download/pupil-absence-in-schools-in-england/2016-17/absence_in_prus.csv', 'absence_in_prus.csv')
  downloaded_file_should_have_first_line('test-results/downloads/absence_in_prus.csv', "time_identifier,time_period,geographic_level,country_code,country_name,region_code,region_name,old_la_code,new_la_code,la_name,school_type,num_schools,enrolments,sess_possible,sess_overall,sess_authorised,sess_unauthorised,sess_overall_percent,sess_authorised_percent,sess_unauthorised_percent,enrolments_pa10_exact,enrolments_pa10_exact_percent,sess_auth_illness,sess_auth_appointments,sess_auth_religious,sess_auth_study,sess_auth_traveller,sess_auth_holiday,sess_auth_ext_holiday,sess_auth_excluded,sess_auth_other,sess_auth_totalreasons,sess_unauth_holiday,sess_unauth_late,sess_unauth_other,sess_unauth_noyet,sess_unauth_totalreasons,sess_overall_totalreasons")
