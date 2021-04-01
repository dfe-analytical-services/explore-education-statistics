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
import math
from pathlib import Path
from dotenv import load_dotenv
import zipfile
import glob 
from os.path import basename

"""
Instructions
- place a zip file (containing a data and a meta file) in an archive file called 'archive.zip' in the root of ~/publisher-tests
- copy the .env.example to .env ('cp .env.example .env')
- get a jwt token from Admin EES & place in .env file
- get the topic ID from the &topicid query parameter on Admin EES 
- place the API url value in the .env file (i.e. https://localhost:5021, dev Admin URL etc.). Do not leave a leading slash at the end of this URL. 
- Command to run publisher script: pipenv run python publisher.py 
- use the -c option to upload more than one subject (i.e. pipenv run python publisher.py -c 5)
"""

parser = argparse.ArgumentParser(prog="pipenv run python publisher.py", description="Used to load test importer / publisher")

parser.add_argument("-c", "--count",
                    dest="count",
                    default=1,
                    type=int,
                    help="number of subjects to upload")

args = parser.parse_args()


def create_publication(api_url):
    run_identifier = datetime.datetime.utcnow().strftime('%Y%m%d-%H%M%S')
    print(f'Starting load test with RUN_IDENTIFIER: {run_identifier}')
    response = requests.request(
        "POST",
        url=f'{api_url}/api/publications',
        verify=False,
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {jwt_token}",
        },
        json={
            "title": f"publisher-test_{run_identifier}",
            "topicId": f"{topic_id}", 
            "contact": {
                "contactName": f"publish-test_{run_identifier}",
                "contactTelNo": "123456789",
                "teamEmail": f"ui_test@email.com",
                "teamName": f"ui_test-{run_identifier}",
            },    
        },
    )
    if response.status_code not in {200, 300}:
        print(f'{colors.FAIL}create_publication status code was {response.status_code}. JWT has expired.')
        sys.exit()

    publication_id = (response.json()['id'])
    return publication_id


def create_release(api_url, publication_id):
    response = requests.request(
        "POST",
        url=f'{api_url}/api/publications/{publication_id}/releases',
        verify=False,
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {jwt_token}",
        },
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
    print(f'{colors.SUCCESS}Found Release ID >', release_id)
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
    file_id = response.json()['id']
    print(f'{colors.CYAN}===============================')
    print(f'Subject Size: >', response.json()['size'])
    print(f'Num of rows: >', response.json()['rows'])
    print(f'{colors.CYAN}================================')
    return file_id

def check_subject_status(random_identifier, file_id, api_url, release_id):
    response = requests.request(
        "GET",
        url=f'{api_url}/api/release/{release_id}/data/{file_id}/import/status',
        verify=False,
        headers={
            "Authorization": f"Bearer {jwt_token}",
            "Content-Type": "application/json",
        },
    )
    if(response.json()['status'] == 'NOT_FOUND'): 
        print(f'{colors.FAIL}Failed to find subject. Trying again to find import status')
        check_subject_status(random_identifier, file_id, api_url, release_id)

    elif(response.json()['status'] == 'COMPLETE'):
        global subject_end_time 
        subject_end_time = time.perf_counter()
        subject_total_time = subject_end_time - subject_start_time
        print(f'PUBLICATION URL > {api_url}/publication/{publication_id}/release/{release_id}/data')
        print(f'{colors.CYAN} Subject upload elapsed time > {subject_total_time:.2f} seconds'  )
    else:
        status = response.json()['status']
        if response.json()['errors']:
            print(f'check_subject_status errors:', response.json()['errors'])
            sys.exit()
        else: 
            percentageComplete = response.json()['percentageComplete']
            print(f'{colors.SUCCESS}subject is in stage >', status, flush=True)
            print(f'percentage > {percentageComplete}% ', flush=True)
            time.sleep(2)
        check_subject_status(random_identifier, file_id, api_url, release_id)

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

def get_subject_id_list(api_url):
    response = requests.request(
    "GET",
    url=f'{api_url}/api/release/{release_id}/meta-guidance',
    verify=False,
    headers={
        "Authorization": f'Bearer {jwt_token}',
        "Content-Type": "application/json",
        }
    )
    subject_id_list = (response.json()['subjects'])
    return subject_id_list

def add_meta_guidance(subject_list, api_url, release_id):
    request_subjects = []
    for subject in subject_list:
        request_subjects.append({
            "id": f'{subject["id"]}',
            "content": "<p>meta guidance publisher tests</p>",
        })

    response = requests.request(
        "PATCH",
        url=f'{api_url}/api/release/{release_id}/meta-guidance',
        verify=False,
        headers={
            "Authorization": f'Bearer {jwt_token}',
            "Content-Type": "application/json",
        },
        json={
            "content": f"publisher tests main guidance content",
            "subjects": request_subjects,
        },
    ) 

def get_final_release_details(api_url, release_id):
    response = requests.request(
        "GET",
        url=f'{api_url}/api/releases/{release_id}',
        verify=False, 
        headers={
        "Authorization": f'Bearer {jwt_token}',
        "Content-Type": "application/json",
        },
    )
    release_data = response.json()
    return release_data

def publish_release(api_url, release_id, release_data):
    test_date = datetime.datetime.today().strftime('%Y-%m-%d')
    response = requests.request(
        "PUT",
        url=f'{api_url}/api/releases/{release_id}',
        verify=False,
        headers={
        "Authorization": f'Bearer {jwt_token}',
        "Content-Type": "application/json",
        },
        json={
        "amendment": "False",
        "contact": {
            "id": f"{release_data['contact']['id']}",
            "teamName": f"{release_data['contact']['teamName']}",
            "teamEmail": f"{release_data['contact']['teamEmail']}",
            "contactName": f"{release_data['contact']['contactName']}",
            "contactTelNo": f"{release_data['contact']['contactTelNo']}",
        },
        "id": f"{release_data['id']}",
        "internalReleaseNote": "Approved by publisher tests",
        "latestRelease": "True",
        "live": "False",
        "publicationId": f"{release_data['publicationId']}",
        "publicationTitle": f"{release_data['publicationTitle']}",
        "publicationSlug": f"{release_data['publicationSlug']}",
        "publishScheduled": test_date,
        "publishMethod": "Immediate",
        "releaseName": f"{release_data['releaseName']}",
        "slug": f"{release_data['slug']}",
        "status": "Approved",
        "timePeriodCoverage": {
            "value": f"{release_data['timePeriodCoverage']['value']}",
            "label": f"{release_data['timePeriodCoverage']['label']}"
        },
        "title": f"{release_data['title']}",
        "type": {
            "id": f"{release_data['type']['id']}",
            "title": f"{release_data['type']['title']}"
        },
        "typeId": f"{release_data['typeId']}",
        "yearTitle": f"{release_data['yearTitle']}",
        }
    )
    if response.status_code == 200:
        print(f'{colors.SUCCESS}release started successfully.')
        print(f'{colors.CYAN}getting release status')
        get_release_status(api_url, release_id)
    else: 
        print(f'{colors.FAIL}publish_release errors: > {response.json()}')
        print(f'{colors.FAIL}status code was: > {response.status_code}')

def get_release_status(api_url, release_id):
    response = requests.request(
        "GET",
        url=f'{api_url}/api/releases/{release_id}/status', 
        verify=False,
        headers={
            "Authorization": f"Bearer {jwt_token}",
            "content-type": "application/json"
        },
    )
    global publish_start_time
    publish_start_time = time.perf_counter()
    try:
        data = json.loads(response.text)
    except:
        print(f'{colors.FAIL} Failed to load response json. Trying again in 3 seconds')
        time.sleep(3)
        get_release_status(api_url, release_id)
    else:

        if(data['overallStage'] != 'Complete'):
            overall_stage = data['overallStage']
            print(f'{colors.SUCCESS}======================')
            print(f'data stage > ', data['dataStage'])
            print(f'content stage >' , data['contentStage'])
            print(f'publishing stage >',   data['publishingStage'])
            print(f'file stage >',  data['filesStage'])
            print(f'overall stage >',  data['overallStage'])
            print(f'Last updated at > ',  data['lastUpdated'])
            print(f'{colors.SUCCESS}======================')
            time.sleep(2)
            get_release_status(api_url, release_id)

        elif data['overallStage'] == 'Complete': 
            publish_end_time = time.perf_counter()
            publish_total_time = publish_end_time - publish_start_time
            print(f'{colors.CYAN} publication elapsed time > {publish_total_time} minutes'  )



if __name__ == "__main__":

    for zip_file in Path('.').glob('test-*.zip'):
        if(zip_file):
            os.remove(zip_file)

    
    if not os.path.exists('test-results'): 
        os.makedirs('test-results')

    if not os.path.exists(f'test-files'): 
        os.makedirs('test-files')

    # Uncomment the below to send STDOUT to a log file 

    # rand_id = random.randint(0,100000)
    # log = open(f'test-results/publisher-test-results-{datetime.date.today()}-{rand_id}.txt', 'a+')
    # sys.stdout = log

    # To prevent InsecureRequestWarning
    requests.packages.urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
    class colors:
        WARNING = '\033[93m' 
        FAIL = '\033[91m' 
        SUCCESS = '\033[92m'
        CYAN = '\033[96m'

    load_dotenv('.env')
    api_url = os.getenv('API_URL')
    jwt_token = os.getenv('JWT_TOKEN')
    topic_id = os.getenv('TOPIC_ID')
    assert api_url is not None
    assert jwt_token is not None
    assert topic_id is not None



    publication_id = create_publication(api_url)
    release_id = create_release(api_url, publication_id)

    for i in range (args.count):
        random_identifier = random.randint(0, 1000000)
        rename_csv_file(random_identifier)

        for file in Path("./test-files").glob("*.csv"):
            os.remove(file)

        final_data_file = open(f'test-{random_identifier}.zip', 'rb')

        file_id = add_subject(random_identifier, final_data_file, api_url, release_id)
        check_subject_status(random_identifier, file_id, api_url, release_id)

    subject_list = get_subject_id_list(api_url)
    add_meta_guidance(subject_list, api_url, release_id)
    release_data = get_final_release_details(api_url, release_id)
    publish_release(api_url, release_id, release_data)