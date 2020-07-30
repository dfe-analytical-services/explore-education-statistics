#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Run 'python run_tests.py -h' to see argument options
"""

import argparse
import cProfile
import json
import os
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
from scripts.get_auth_tokens import get_identity_info

current_dir = Path(__file__).absolute().parent
os.chdir(current_dir)

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
pyderman.install(file_directory='./webdriver/',
                 filename='chromedriver',
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

# Auth not required with general_public tests
if args.tests and "general_public" not in args.tests:
    def authenticate_user(user, email, password, clear_existing=True):
        assert user and email and password

        local_storage_name = f'IDENTITY_LOCAL_STORAGE_{user}'
        cookie_name = f'IDENTITY_COOKIE_{user}'

        local_storage_file = Path(f'{local_storage_name}.json')
        cookie_file = Path(f'{cookie_name}.json')

        if clear_existing:
            local_storage_file.unlink(True)
            cookie_file.unlink(True)

        if local_storage_file.exists() and cookie_file.exists():
            print(f'Getting {user} authentication information from local files... ', flush=True)

            os.environ[local_storage_name] = local_storage_file.read_text()
            os.environ[cookie_name] = cookie_file.read_text()
        else:
            print(f'Logging in to obtain {user} authentication information... ', flush=True)

            os.environ[local_storage_name], os.environ[cookie_name] = get_identity_info(
                url=os.getenv('ADMIN_URL'),
                email=email,
                password=password,
                chromedriver_version=args.chromedriver_version
            )

            # Cache auth info to files for efficiency
            local_storage_file.write_text(os.environ[local_storage_name])
            cookie_file.write_text(os.environ[cookie_name])

            print('Done!', flush=True)

        assert os.getenv(local_storage_name) is not None
        assert os.getenv(cookie_name) is not None


    def setup_authentication(clear_existing=False):
        # Don't need BAU user if running general_public tests
        if "general_public" not in args.tests:
            authenticate_user(
                user='BAU',
                email=os.getenv('ADMIN_EMAIL'),
                password=os.getenv('ADMIN_PASSWORD'),
                clear_existing=clear_existing
            )

        # Don't need analyst user if running admin/bau or admin_and_public/bau tests
        if f"{os.sep}bau" not in args.tests:
            authenticate_user(
                user='ANALYST',
                email=os.getenv('ANALYST_EMAIL'),
                password=os.getenv('ANALYST_PASSWORD'),
                clear_existing=clear_existing
            )


    def admin_request(method, endpoint, body=None):
        assert method and endpoint
        assert os.getenv('IDENTITY_LOCAL_STORAGE_BAU') is not None
        if method == 'POST':
            assert body is not None, 'POST requests require a body'

        # To prevent InsecureRequestWarning
        requests.packages.urllib3.disable_warnings()

        jwt_token = json.loads(os.getenv('IDENTITY_LOCAL_STORAGE_BAU'))['access_token']
        headers = {
            'Content-Type': 'application/json',
            'Authorization': f'Bearer {jwt_token}',
        }
        response = requests.request(method, endpoint, headers=headers, json=body, verify=False)
        if response.status_code in {401, 403}:
            print('Attempting re-authentication...', flush=True)

            # Delete identify files and re-attempt to fetch them
            setup_authentication(clear_existing=True)
            jwt_token = json.loads(os.environ['IDENTITY_LOCAL_STORAGE_BAU'])['access_token']
            headers = {
                'Content-Type': 'application/json',
                'Authorization': f'Bearer {jwt_token}',
            }
            response = requests.request(method, endpoint, headers=headers, json=body, verify=False)

            assert response.status_code not in {401, 403}, \
                'Failed to reauthenticate.'
        assert response.status_code == 200, 'Admin request status_code wasn\'t 200!'
        return response

    def get_test_themes():
        assert os.getenv('ADMIN_URL') is not None

        get_themes_endpoint = f'{os.getenv("ADMIN_URL")}/api/me/themes'
        return admin_request('GET', get_themes_endpoint)


    def create_test_theme():
        assert os.getenv('ADMIN_URL') is not None

        create_theme_endpoint = f'{os.getenv("ADMIN_URL")}/api/theme'
        body = {'title': 'Test theme', 'summary': 'Test theme summary'}
        return admin_request('POST', create_theme_endpoint, body)


    def create_test_topic(theme_id):
        assert theme_id
        assert os.getenv('ADMIN_URL') is not None
        assert os.getenv('RUN_IDENTIFIER') is not None

        create_topic_endpoint = f'{os.getenv("ADMIN_URL")}/api/theme/{theme_id}/topics'
        body = {'title': f'UI test topic {os.getenv("RUN_IDENTIFIER")}'}
        return admin_request('POST', create_topic_endpoint, body)

    setup_authentication()

    # NOTE(mark): Tests that alter data only occur on local and dev environments
    if args.env in ['local', 'dev']:
        runIdentifier = datetime.datetime.utcnow().strftime('%Y%m%d-%H%M%S')

        os.environ['RUN_IDENTIFIER'] = runIdentifier
        print(f'Starting tests with RUN_IDENTIFIER: {runIdentifier}')

        get_themes_resp = get_test_themes()
        test_theme_guid = None
        for theme in get_themes_resp.json():
            if theme['title'] == 'Test theme':
                test_theme_guid = theme['id']
                break
        if not test_theme_guid:
            create_theme_resp = create_test_theme()
            test_theme_guid = create_theme_resp.json()['id']
        assert test_theme_guid is not None, 'test_theme_guid hasn\'t been set!'

        create_test_topic(test_theme_guid)

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

robotArgs += ["-v", "browser:" + args.browser]
robotArgs += [args.tests]

# Remove any existing test results
if Path('test-results').exists():
    shutil.rmtree("test-results")

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
