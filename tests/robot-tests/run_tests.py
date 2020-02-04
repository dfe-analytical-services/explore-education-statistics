#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Run 'python run_tests.py -h' to see argument options
"""

import os
import argparse
from robot import run_cli as robot_run_cli
from pabot.pabot import main as pabot_run_cli
import cProfile
import pstats
import scripts.keyword_profile as kp
import chromedriver_install as cdi
from dotenv import load_dotenv
import time
import requests
import json

# Parse arguments
parser = argparse.ArgumentParser(prog="pipenv run python run_tests.py",
                                 description="Use this script to run the UI tests, locally or as part of the CI pipeline, against the environment of your choosing")
parser.add_argument("-b", "--browser",
                    dest="browser",
                    default="chrome",
                    choices=["chrome", "firefox"],
                    help="name of the browser you wish to run the tests with (NOTE: Only chromedriver is automatically installed!)")
parser.add_argument("-i", "--interp",
                    dest="interp",
                    default="pabot",
                    choices=["pabot", "robot"],
                    help="interpreter to use to run the tests")
parser.add_argument("-e", "--env",
                    dest="env",
                    default="test",
                    choices=["local", "dev", "dev02", "dev03", "test", "prod", "ci"],
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
                    default="78.0.3904.70",
                    metavar="{version}",
                    help="specify which version of chromedriver to use")
"""
NOTE(mark): The admin password to access the admin app is stored in the CI pipeline 
            as a secret variable, which means it cannot be accessed like a normal 
            environment variable, and instead must be passed to this script as 
            an argument.
"""
parser.add_argument("--admin-pass",
                    dest="admin_pass",
                    default=None,
                    help="manually specify the admin password")
args = parser.parse_args()

if args.admin_pass:
    os.environ['ADMIN_PASSWORD'] = args.admin_pass

# Default values
timeout = 20
implicit_wait = 20

# Install chromedriver and add it to PATH
cdi.install(file_directory='./webdriver/',
            verbose=False,
            chmod=True,
            overwrite=False,
            version=args.chromedriver_version)
os.environ["PATH"] += os.pathsep + os.getcwd() + os.sep + 'webdriver'

# Set robotArgs
robotArgs = ["--outputdir", "test-results/",
             "--exclude", "Failing",
             "--exclude", "UnderConstruction"]
if args.tags:
    robotArgs += ["--include", args.tags]

if args.ci:
    robotArgs += ["--xunit", "xunit", "-v", "timeout:" + str(timeout), "-v", "implicit_wait:" + str(implicit_wait)]
    # NOTE(mark): Ensure secrets aren't visible in CI logs/reports
    robotArgs += ["--removekeywords", "name:operatingsystem.environment variable should be set"]
    robotArgs += ['--removekeywords', 'name:common.user goes to url']  # To hide basic auth credentials
else:
    load_dotenv(os.path.join(os.path.dirname(__file__), '.env.' + args.env))
    assert os.getenv('PUBLIC_URL') is not None
    assert os.getenv('ADMIN_URL') is not None
    assert os.getenv('ADMIN_EMAIL') is not None
    assert os.getenv('ADMIN_PASSWORD') is not None

if args.tests and "general_public" not in args.tests:  # Auth not required with general_public tests
    print('Getting get_identity_info function from get_auth_tokens.py script...', end='')
    # NOTE(mark): Because you cannot import from a parent dir, we do this...
    if os.path.exists(f'..{os.sep}..{os.sep}useful-scripts{os.sep}auth-tokens{os.sep}get_auth_tokens.py'):
        f = open(f'..{os.sep}..{os.sep}useful-scripts{os.sep}auth-tokens{os.sep}get_auth_tokens.py', 'r')
    elif os.path.exists(f'..{os.sep}auth-token-script{os.sep}get_auth_tokens.py'):  # For pipeline
        f = open(f'..{os.sep}auth-token-script{os.sep}get_auth_tokens.py', 'r')
    assert f is not None, 'Failed to open file get_auth_tokens.py!'
    get_auth_tokens_script = f.read()
    globals()['__name__'] = '__test_runner__'
    exec(get_auth_tokens_script, globals(), locals())
    assert callable(get_identity_info)
    print('done!')

    # For BAU user
    if os.path.exists('IDENTITY_LOCAL_STORAGE_BAU.txt') and os.path.exists('IDENTITY_COOKIE_BAU.txt'):
        print('Getting BAU user authentication information from local files...', end='')
        with open('IDENTITY_LOCAL_STORAGE_BAU.txt', 'r') as ls_file:
            os.environ['IDENTITY_LOCAL_STORAGE_BAU'] = ls_file.read()
        with open('IDENTITY_COOKIE_BAU.txt', 'r') as cookie_file:
            os.environ['IDENTITY_COOKIE_BAU'] = cookie_file.read()
        print('done!')
    else:
        print('Logging in to obtain BAU user authentication information...', end='', flush=True)
        os.environ["IDENTITY_LOCAL_STORAGE_BAU"], os.environ['IDENTITY_COOKIE_BAU'] = get_identity_info(os.getenv('ADMIN_URL'), os.getenv('ADMIN_EMAIL'), os.getenv('ADMIN_PASSWORD'), chromedriver_version=args.chromedriver_version)

        # Save auth info to files for efficiency
        with open('IDENTITY_LOCAL_STORAGE_BAU.txt', 'w') as ls_file:
            ls_file.write(os.environ['IDENTITY_LOCAL_STORAGE_BAU'])
        with open('IDENTITY_COOKIE_BAU.txt', 'w') as cookie_file:
            cookie_file.write(os.environ['IDENTITY_COOKIE_BAU'])
        print(' done!')
    assert os.getenv('IDENTITY_LOCAL_STORAGE_BAU') is not None
    assert os.getenv('IDENTITY_COOKIE_BAU') is not None

    # For Prerelease user
    #if os.path.exists('IDENTITY_LOCAL_STORAGE_PR.txt') and os.path.exists('IDENTITY_COOKIE_PR.txt'):
    #    print('Getting prerelease user authentication information from local files...', end='')
    #    with open('IDENTITY_LOCAL_STORAGE_PR.txt', 'r') as ls_file:
    #        os.environ['IDENTITY_LOCAL_STORAGE_PR'] = ls_file.read()
    #    with open('IDENTITY_COOKIE_PR.txt', 'r') as cookie_file:
    #        os.environ['IDENTITY_COOKIE_PR'] = cookie_file.read()
    #    print('done!')
    #else:
    #    print('Logging in to obtain prerelease user authentication information...', end='', flush=True)
    #    os.environ["IDENTITY_LOCAL_STORAGE_PR"], os.environ['IDENTITY_COOKIE_PR'] = get_identity_info(os.getenv('ADMIN_URL'), os.getenv('PRERELEASE_EMAIL'), os.getenv('PRERELEASE_PASSWORD'), first_name="Prerelease3", last_name="EESADMIN", chromedriver_version=args.chromedriver_version)

    #    # Save auth info to files for efficiency
    #    with open('IDENTITY_LOCAL_STORAGE_PR.txt', 'w') as ls_file:
    #        ls_file.write(os.environ['IDENTITY_LOCAL_STORAGE_PR'])
    #    with open('IDENTITY_COOKIE_PR.txt', 'w') as cookie_file:
    #        cookie_file.write(os.environ['IDENTITY_COOKIE_PR'])
    #    print(' done!')
    #assert os.getenv('IDENTITY_LOCAL_STORAGE_PR') is not None
    #assert os.getenv('IDENTITY_COOKIE_PR') is not None

    # NOTE(mark): Tests that alter data only occur on local and dev environments
    if args.env in ['local', 'dev']:
        requests.packages.urllib3.disable_warnings()  # To prevent InsecureRequestWarning
        # Create topic to be used by UI tests
        run_identifier = str(time.time()).split('.')[0]
        create_topic_endpoint = f'{os.getenv("ADMIN_URL")}/api/theme/449d720f-9a87-4895-91fe-70972d1bdc04/topics'
        jwt_token = json.loads(os.environ['IDENTITY_LOCAL_STORAGE_BAU'])['access_token']
        headers = {
            'Content-Type': 'application/json',
            'Authorization': f'Bearer {jwt_token}',
        }
        body = {'title': f'UI test topic {run_identifier}'}
        r = requests.post(create_topic_endpoint, headers=headers, json=body, verify=False)

        #print('r.status_code', r.status_code)
        #print('r.text', r.text)
        assert r.status_code != 401, 'Failed to authenticate to create topic! Delete robot-tests/IDENTITY_*.txt files?'
        assert r.status_code == 200, 'Failed to create topic! Have you created the Test theme?'
        os.environ['RUN_IDENTIFIER'] = run_identifier
        assert os.getenv('RUN_IDENTIFIER') is not None

if args.env == 'local':
    robotArgs += ['--include', 'Local']
    robotArgs += ['--exclude', 'NotAgainstLocal']
if args.env == 'dev':
    robotArgs += ['--include', 'Dev']
if args.env == 'test':
    robotArgs += ['--include', 'Test',
                  '--exclude', 'AltersData']
if args.env in ['dev03', 'prod']:
    robotArgs += ['--include', 'Prod',
                  '--exclude', 'AltersData']

if args.visual:
    robotArgs += ["-v", "headless:0"]
else:
    robotArgs += ["-v", "headless:1"]

robotArgs += ["-v", "browser:" + args.browser]
robotArgs += [args.tests]

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
        print("\nProfiling logs created in test-results/")
    else:
        robot_run_cli(robotArgs)
elif args.interp == "pabot":
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
        print("\nProfiling logs created in test-results/")
    else:
        pabot_run_cli(robotArgs)
