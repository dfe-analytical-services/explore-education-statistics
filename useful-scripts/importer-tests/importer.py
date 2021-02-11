import os
import time
import json
import requests
import argparse
import csv
import urllib3
import datetime
import random 
import sys
from pathlib import Path
from dotenv import load_dotenv
import zipfile
import glob 
from os.path import basename

""" 
Instructions

- place a zip file (containing a data and a meta file) in an archive file called 'archive.zip' in the root of ~/importer-tests

- move the .env.example to .env ('mv .env.example .env')

- get a jwt token from Admin EES & place in .env file

- get the topic ID from the &topicid query parameter on Admin EES 

- place the API url value in the .env file (i.e. https://localhost:5021, dev Admin URL etc.). Do not leave a leading slash at the end of this URL. 

- Command to run importer script: pipenv run python importer.py 

- use the -c option if you want to import more than one subject (i.e. 'pipenv run python importer.py -c 5')
""" 

parser = argparse.ArgumentParser(prog="pipenv run python importer.py", description="Used to load test importer")

parser.add_argument("-c", "--count",
                    dest="count",
                    default=1,
                    type=int,
                    help="num of subjects to upload")

args = parser.parse_args()


def create_publication(api_url):
    run_identifier = datetime.datetime.utcnow().strftime('%Y%m%d-%H%M%S')
    print(f'Starting load test with RUN_IDENTIFIER: {run_identifier}')
    response = requests.request(
        "POST",
        url=f'{api_url}/api/publications',
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {jwt_token}",
        },
        json={
            "title": f"importer-test_{run_identifier}",
            "topicId": f"{topic_id}", 
            "contact": {
                "contactName": f"import-test_{run_identifier}",
                "contactTelNo": "123456789",
                "teamEmail": f"ui_test@email.com",
                "teamName": f"ui_test-{run_identifier}",
            },    
        },
        verify=False,
    )
    if response.status_code not in {200, 300}:
        print(f'create_publication status code was {response.status_code}. JWT needs updating?')
        sys.exit()

    publication_id = (response.json()['id'])
    return publication_id


def create_release(api_url, publication_id):
    response = requests.request(
        "POST",
        url=f'{api_url}/api/publications/{publication_id}/releases',
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {jwt_token}",
        },
        verify=False,
        json={
            "publicationId": f"{publication_id}",
            "timePeriodCoverage": {
                "value": "AY"
            },
            "releaseName": f"{2010 + random.randint(0, 100)}",
            "typeId": "1821abb8-68b0-431b-9770-0bea65d02ff0",
            "publicationId": "6a282610-bfed-444d-6436-08d8b1808078",
            "templateReleaseId": ""
        }
    )

    if response.status_code not in {200, 300}:
        print(f'create_release response isn\'t 200 or 300! It is {response.status_code}')
        sys.exit()

    release_id = response.json()['id']
    print(f'Found Release ID >', release_id)
    return release_id


def add_subject(random_identifier, final_data_file, api_url, release_id):
    global subject_start_time
    subject_start_time = time.perf_counter()
    response = requests.request(
        "POST",
        url=f'{api_url}/api/release/{release_id}/zip-data?name=test-subject-{random_identifier}',
        verify=False,
        headers={
            "Authorization": f"Bearer {jwt_token}",
        },
        files = {
            "zipFile": final_data_file
        }
    )
    subject_id = response.json()['id']
    return subject_id 

def check_subject_status(random_identifier, api_url, release_id):
    response = requests.request(
        "GET",
        url=f'{api_url}/api/release/{release_id}/data/{subject_id}/import/status',
        verify=False,
        headers={
            "Authorization": f"Bearer {jwt_token}",
            "Content-Type": "application/json",
        },
    )
    if(response.json()['status'] == 'NOT_FOUND'): 
        print('failed to find subject. Trying again to find import status')
        check_subject_status(random_identifier, api_url, release_id)

    elif(response.json()['status'] == 'COMPLETE'):
        subject_end_time = time.perf_counter()
        print(f'PUBLICATION URL > {api_url}/publication/{publication_id}/release/{release_id}/data')
        print(f'elapsed time > ', subject_end_time - subject_start_time)
        print(f'Subject size: >', response.json()['size'])
        print(f'Num of rows: >', response.json()['rows'])

    else:
        status = response.json()['status']
        if response.json()['errors']:
            print(f'errors occured:', response.json()['errors'])
            sys.exit()
        else: 
            percentageComplete = response.json()['percentageComplete']
            print(f'subject is in stage >', status, flush=True)
            print(f'percentage > {percentageComplete}% ', flush=True)
            time.sleep(2)
        # uncomment the below to send STDOUT to log file
        # log = open(f'test-results/importer-log-{datetime.date.today()}.txt', 'a+')
        # sys.stdout = log
        check_subject_status(random_identifier, api_url, release_id)


def rename_csv_file(random_identifier):
    try: 
        def get_all_file_paths(directory): 
            file_paths = [] 
            for root, directories, files in os.walk(directory): 
                for filename in files: 
                    filepath = os.path.join(root, filename) 
                    file_paths.append(filepath) 
            return file_paths  

        with zipfile.ZipFile('archive.zip', 'r') as zip_file:
            zip_file.extractall('./test-files')

        meta_filepath = glob.glob(f'./test-files/*.meta.csv')[0]
        meta_filename = os.path.basename(meta_filepath)
        os.rename(f'./test-files/{meta_filename}', f'./test-files/subject-{random_identifier}.meta.csv')

        data_filepath = glob.glob(f'./test-files/*.csv')[0]
        data_filename = os.path.basename(data_filepath)
        os.rename(f'./test-files/{data_filename}', f'./test-files/subject-{random_identifier}.csv')

        with zipfile.ZipFile(f'test-{random_identifier}.zip', 'w') as zip: 
            test_dir = './test-files'
            file_path = get_all_file_paths(test_dir)
            for individual_file in file_path:
                zip.write(individual_file, basename(individual_file))
    except Exception as e:
        print(f'Error: {e}')


if __name__ == "__main__":
    # To prevent InsecureRequestWarning
    requests.packages.urllib3.disable_warnings()

    load_dotenv('.env')
    api_url = os.getenv('API_URL')
    jwt_token = os.getenv('JWT_TOKEN')
    topic_id = os.getenv('TOPIC_ID')
    assert api_url is not None
    assert jwt_token is not None
    assert topic_id is not None
    
    # clean zip files
    for zip_file in Path('.').glob('test-*.zip'):
        if(zip_file):
            os.remove(zip_file)

    # make directory
    if not os.path.exists('test-results'): 
        os.makedirs('test-results')

    if not os.path.exists(f'test-files'): 
        os.makedirs('test-files')

    publication_id = create_publication(api_url)
    release_id = create_release(api_url, publication_id)

    for i in range (args.count):
        random_identifier = random.randint(0, 1000000)
        rename_csv_file(random_identifier)

        for file in Path("./test-files").glob("*.csv"):
            os.remove(file)

        final_data_file = open(f'test-{random_identifier}.zip', 'rb')

        subject_id = add_subject(random_identifier, final_data_file, api_url, release_id)
        check_subject_status(random_identifier, api_url, release_id)