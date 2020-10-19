#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Run 'python run_tests.py -h' to see argument options
"""

import argparse
import cProfile
import json
import os
import platform
import pstats
import shutil
import datetime
from pathlib import Path

import pyderman
import requests
from dotenv import load_dotenv
from pabot.pabot import main as pabot_run_cli
from robot import run_cli as robot_run_cli

import scripts.keyword_profile as kp
from tests.libs.setup_auth_variables import setup_auth_variables

current_dir = Path(__file__).absolute().parent
os.chdir(current_dir)

# This is super awkward but we have to explicitly
# add the current directory to PYTHONPATH otherwise
# the subprocesses started by pabot will not be able
# to locate lib modules correctly for some reason.
pythonpath = os.getenv('PYTHONPATH')

if pythonpath:
    os.environ['PYTHONPATH'] += f':{str(current_dir)}'
else:
    os.environ['PYTHONPATH'] = str(current_dir)

# Parse arguments
parser = argparse.ArgumentParser(prog="pipenv run python run_tests.py",
                                 description="Use this script to run the UI tests, locally or as part of the CI pipeline, against the environment of your choosing")
parser.add_argument("-b", "--browser",
                    dest="browser",
                    default="chrome",
                    choices=["chrome", "firefox", "ie"],
                    help="name of the browser you wish to run the tests with (NOTE: Only chromedriver is automatically installed!)")
parser.add_argument("-i", "--interp",
                    dest="interp",
                    default="pabot",
                    choices=["pabot", "robot"],
                    help="interpreter to use to run the tests")
parser.add_argument("--processes",
                    dest="processes",
                    help="how many processes should be used when using the pabot interpreter")
parser.add_argument("-e", "--env",
                    dest="env",
                    default="test",
                    choices=["local", "dev", "test", "preprod", "prod", "ci"],
                    help="the environment to run the tests against")
parser.add_argument("-f", "--file",
                    dest="tests",
                    metavar="{file/dir}",
                    default="tests/",
                    help="test suite or folder of tests suites you wish to run")
parser.add_argument("-t", "--tags",
                    dest="tags",
                    nargs="?",
                    metavar="{tag(s)}",
                    help="specify tests you wish to run by tag")
parser.add_argument("-v", "--visual",
                    dest="visual",
                    action="store_true",
                    help="display browser window that the tests run in")
parser.add_argument("-p", "--profile",
                    dest="profile",
                    action="store_true",
                    help="output profiling information")
parser.add_argument("--ci",
                    dest="ci",
                    action="store_true",
                    help="specify that the test are running as part of the CI pipeline")
parser.add_argument("--chromedriver",
                    dest="chromedriver_version",
                    metavar="{version}",
                    help="specify which version of chromedriver to use")
parser.add_argument("--disable-teardown",
                    dest="disable_teardown",
                    help="disable tearing down of any test data after completion",
                    action='store_true')

"""
NOTE(mark): The admin and analyst passwords to access the admin app are stored in the CI pipeline 
            as secret variables, which means they cannot be accessed like normal 
            environment variables, and instead must be passed to this script as 
            arguments.
"""
parser.add_argument("--admin-pass",
                    dest="admin_pass",
                    default=None,
                    help="manually specify the admin password")
parser.add_argument("--analyst-pass",
                    dest="analyst_pass",
                    default=None,
                    help="manually specify the analyst password")
args = parser.parse_args()

if args.admin_pass:
    os.environ['ADMIN_PASSWORD'] = args.admin_pass

if args.analyst_pass:
    os.environ['ANALYST_PASSWORD'] = args.analyst_pass

# Default values
timeout = 30
implicit_wait = 5

# Install chromedriver and add it to PATH
chromedriver_filename = 'chromedriver.exe' if platform.system() == "Windows" else 'chromedriver'
pyderman.install(file_directory='./webdriver/',
                 filename=chromedriver_filename,
                 verbose=False,
                 chmod=True,
                 overwrite=False,
                 version=args.chromedriver_version)

os.environ["PATH"] += os.pathsep + str(Path('webdriver').absolute())

# Set robotArgs
robotArgs = ["--outputdir", "test-results/",
             # "--exitonfailure",
             "--exclude", "Failing",
             "--exclude", "UnderConstruction"]
if args.tags:
    robotArgs += ["--include", args.tags]

if args.ci:
    robotArgs += ["--xunit", "xunit", "-v", "timeout:" + str(timeout), "-v",
                  "implicit_wait:" + str(implicit_wait)]
    # NOTE(mark): Ensure secrets aren't visible in CI logs/reports
    robotArgs += ["--removekeywords", "name:operatingsystem.environment variable should be set"]
    robotArgs += ['--removekeywords',
                  'name:common.user goes to url']  # To hide basic auth credentials
else:
    load_dotenv('.env.' + args.env)
    assert os.getenv('PUBLIC_URL') is not None
    assert os.getenv('ADMIN_URL') is not None
    assert os.getenv('ADMIN_EMAIL') is not None
    assert os.getenv('ADMIN_PASSWORD') is not None


def admin_request(method, endpoint, body=None):
    assert method and endpoint
    assert os.getenv('ADMIN_URL') is not None
    assert os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN') is not None

    if method == 'POST':
        assert body is not None, 'POST requests require a body'

    # To prevent InsecureRequestWarning
    requests.packages.urllib3.disable_warnings()

    jwt_token = json.loads(os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN'))['access_token']
    headers = {
        'Content-Type': 'application/json',
        'Authorization': f'Bearer {jwt_token}',
    }
    response = requests.request(
        method,
        url=f'{os.getenv("ADMIN_URL")}{endpoint}',
        headers=headers,
        json=body,
        verify=False
    )

    if response.status_code in {401, 403}:
        print('Attempting re-authentication...', flush=True)

        # Delete identify files and re-attempt to fetch them
        setup_authentication(clear_existing=True)
        jwt_token = json.loads(os.environ['IDENTITY_LOCAL_STORAGE_ADMIN'])['access_token']
        response = requests.request(
            method,
            url=f'{os.getenv("ADMIN_URL")}{endpoint}',
            headers={
                'Content-Type': 'application/json',
                'Authorization': f'Bearer {jwt_token}',
            },
            json=body,
            verify=False
        )

        assert response.status_code not in {401, 403}, \
            'Failed to reauthenticate.'

    assert response.status_code < 300, f'Admin request responded with {response.status_code} and {response.text}'
    return response


def get_test_themes():
    return admin_request('GET', '/api/themes')


def create_test_theme():
    return admin_request('POST', '/api/themes', {
        'title': 'Test theme',
        'summary': 'Test theme summary'
    })


def create_test_topic():
    assert os.getenv('TEST_THEME_ID') is not None

    topic_name = f'UI test topic {os.getenv("RUN_IDENTIFIER")}'
    resp = admin_request('POST', '/api/topics', {
        'title': topic_name,
        'themeId': os.getenv('TEST_THEME_ID')
    })

    os.environ['TEST_TOPIC_NAME'] = topic_name
    os.environ['TEST_TOPIC_ID'] = resp.json()['id']


def delete_test_topic():
    if os.getenv('TEST_TOPIC_ID') is not None:
        admin_request('DELETE', f'/api/topics/{os.getenv("TEST_TOPIC_ID")}')


def setup_authentication(clear_existing=False):
    # Don't need BAU user if running general_public tests
    if "general_public" not in args.tests:
        setup_auth_variables(
            user='ADMIN',
            email=os.getenv('ADMIN_EMAIL'),
            password=os.getenv('ADMIN_PASSWORD'),
            clear_existing=clear_existing
        )

    # Don't need analyst user if running admin/bau or admin_and_public/bau tests
    if f"{os.sep}bau" not in args.tests:
        setup_auth_variables(
            user='ANALYST',
            email=os.getenv('ANALYST_EMAIL'),
            password=os.getenv('ANALYST_PASSWORD'),
            clear_existing=clear_existing
        )


# Auth not required with general_public tests
if args.tests and "general_public" not in args.tests:
    setup_authentication()

    # NOTE(mark): Tests that alter data only occur on local and dev environments
    if args.env in ['local', 'dev']:
        runIdentifier = datetime.datetime.utcnow().strftime('%Y%m%d-%H%M%S')

        os.environ['RUN_IDENTIFIER'] = runIdentifier
        print(f'Starting tests with RUN_IDENTIFIER: {runIdentifier}')

        get_themes_resp = get_test_themes()
        test_theme_id = None
        test_theme_name = 'Test theme'

        for theme in get_themes_resp.json():
            if theme['title'] == test_theme_name:
                test_theme_id = theme['id']
                break
        if not test_theme_id:
            create_theme_resp = create_test_theme()
            test_theme_id = create_theme_resp.json()['id']

        os.environ['TEST_THEME_NAME'] = test_theme_name
        os.environ['TEST_THEME_ID'] = test_theme_id

        create_test_topic()

if args.env == 'local':
    robotArgs += ['--include', 'Local']
    robotArgs += ['--exclude', 'NotAgainstLocal']
if args.env == 'dev':
    robotArgs += ['--include', 'Dev']
    robotArgs += ['--exclude', 'NotAgainstDev']
if args.env == 'test':
    robotArgs += ['--include', 'Test',
                  '--exclude', 'AltersData']
if args.env == 'preprod':
    robotArgs += ['--include', 'Preprod',
                  '--exclude', 'AltersData',
                  '--exclude', 'NotAgainstPreProd']

if args.env == 'prod':
    robotArgs += ['--include', 'Prod',
                  '--exclude', 'AltersData']

if args.visual:
    robotArgs += ["-v", "headless:0"]
else:
    robotArgs += ["-v", "headless:1"]

if os.getenv('RELEASE_COMPLETE_WAIT'):
    robotArgs += ["-v", f"release_complete_wait:{os.getenv('RELEASE_COMPLETE_WAIT')}"]

robotArgs += ["-v", "browser:" + args.browser]
robotArgs += [args.tests]

# Remove any existing test results
if Path('test-results').exists():
    shutil.rmtree("test-results")

try:
    # Run tests
    if args.interp == "robot":
        if args.profile:
            # Python profiling
            cProfile.run('robot_run_cli(robotArgs)', 'profile-data')
            stream = open('test-results/python-profiling-results.log', 'w')
            p = pstats.Stats('profile-data', stream=stream)
            p.sort_stats('time')
            # p.sort_stats('cumulative')
            p.print_stats()
            os.remove('profile-data')

            # Keyword profiling
            kp.run_keyword_profile('test-results/output.xml',
                                   printresults=False,
                                   writepath='test-results/keyword-profiling-results.log')
            print("\nProfiling logs created in test-results/", flush=True)
        else:
            robot_run_cli(robotArgs)
    elif args.interp == "pabot":
        if args.processes:
            robotArgs = ["--processes", args.processes] + robotArgs

        if args.profile:
            # Python profiling
            cProfile.run('pabot_run_cli(robotArgs)', 'profile-data')
            stream = open('test-results/python-profiling-results.log', 'w')
            p = pstats.Stats('profile-data', stream=stream)
            p.sort_stats('time')
            # p.sort_stats('cumulative')
            p.print_stats()
            os.remove('profile-data')

            # Keyword profiling
            kp.run_keyword_profile('test-results/output.xml',
                                   printresults=False,
                                   writepath='test-results/keyword-profiling-results.log')
            print("\nProfiling logs created in test-results/", flush=True)
        else:
            pabot_run_cli(robotArgs)
finally:
    if not args.disable_teardown:
        print("Tearing down tests...", flush=True)
        delete_test_topic()

    print("Tests finished!")
