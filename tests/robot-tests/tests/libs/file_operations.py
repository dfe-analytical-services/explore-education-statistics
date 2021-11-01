from robot.libraries.BuiltIn import BuiltIn

sl = BuiltIn().get_library_instance('SeleniumLibrary')
import os
import requests
import zipfile

requests.sessions.HTTPAdapter(
        pool_connections=50,
        pool_maxsize=50,
        max_retries=3
    )
session = requests.Session()

def download_file(link_locator, file_name):
    if not os.path.exists('test-results/downloads'):
        os.makedirs('test-results/downloads')
    link_url = sl.get_element_attribute(link_locator, 'href')
    r = session.get(link_url, allow_redirects=True, stream=True)
    with open(f'test-results/downloads/{file_name}', 'wb') as f:
        f.write(r.content)


def downloaded_file_should_have_first_line(filename, expected_first_line):
    with open(f'test-results/downloads/{filename}', 'r') as f:
        first_line = f.readline().rstrip()
    if first_line != expected_first_line:
        raise AssertionError(
            f'First line of file "{filename}" didn\'t match expected first line: "{expected_first_line}"')


def zip_should_contains_x_files(zipfilename, num_files):
    zip = zipfile.ZipFile(f'test-results/downloads/{zipfilename}')
    files_in_zip = zip.namelist()
    assert len(files_in_zip) == int(num_files), f'Number of files in zip {zipfilename} was {len(files_in_zip)}. The test expected {num_files}!)'


def zip_should_contain_file(zipfilename, filename):
    zip = zipfile.ZipFile(f'test-results/downloads/{zipfilename}')
    files_in_zip = zip.namelist()
    if filename not in files_in_zip:
        raise AssertionError(f'File "{filename}" not found in "{zipfilename}", which contains {str(files_in_zip)}')
