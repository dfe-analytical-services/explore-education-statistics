#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Run 'python run_tests.py -h' to see argument options
"""

import argparse
import cProfile
import datetime
import json
import os
import platform
import pstats
import shutil
from pathlib import Path

import pyderman
import requests
from dotenv import load_dotenv
from pabot.pabot import main as pabot_run_cli
from robot import rebot_cli as robot_rebot_cli
from robot import run_cli as robot_run_cli
import scripts.keyword_profile as kp
from tests.libs.setup_auth_variables import setup_auth_variables
from tests.libs.slack import send_slack_report
from tests.libs.create_emulator_release_files import ReleaseFilesGenerator
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
parser = argparse.ArgumentParser(
    prog="pipenv run python run_tests.py",
    description="Use this script to run the UI tests, locally or as part of the CI pipeline, against the environment of your choosing")
parser.add_argument(
    "-b",
    "--browser",
    dest="browser",
    default="chrome",
    choices=[
        "chrome",
        "firefox",
        "ie"],
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
parser.add_argument("--rerun-failed-tests",
                    dest="rerun_failed_tests",
                    action='store_true',
                    help="rerun individual failed tests and merge results into original run results")
parser.add_argument("--rerun-failed-suites",
                    dest="rerun_failed_suites",
                    action='store_true',
                    help="rerun failed test suites and merge results into original run results")
parser.add_argument("--timeout",
                    default="30",
                    dest="timeout",
                    help="default robot timeout in seconds (default is 30)")
parser.add_argument("--implicit-wait",
                    default="5",
                    dest="implicit_wait",
                    help="default robot implicit wait in seconds (default is 5)")
parser.add_argument("--print-keywords",
                    dest="print_keywords",
                    action='store_true',
                    help="choose to print out keywords as they are started")
parser.add_argument("--enable-slack",
                    dest="enable_slack",
                    action='store_true'
                    )
parser.add_argument("--prompt-to-continue",
                    dest="prompt_to_continue",
                    action='store_true',
                    help="get prompted to continue with test execution upon a failure")
parser.add_argument("--fail-fast",
                    dest="fail_fast",
                    action='store_true',
                    help="stop test execution on failure")
parser.add_argument("--custom-env",
                    dest="custom_env",
                    default=None,
                    help="load a custom .env file (must be in ~/robot-tests directory)")
"""
NOTE(mark): The slack webhook url, and admin and analyst passwords to access to Admin app are
stored in the CI pipeline as secret variables, which means they cannot be accessed as normal
environment variables, and instead must be passed as an argument to this script.
"""
parser.add_argument("--slack-webhook-url",
                    dest="slack_webhook_url",
                    default=None,
                    help="URL for Slack webhook")
parser.add_argument("--admin-pass",
                    dest="admin_pass",
                    default=None,
                    help="manually specify the admin password")
parser.add_argument("--analyst-pass",
                    dest="analyst_pass",
                    default=None,
                    help="manually specify the analyst password")
args = parser.parse_args()

if args.slack_webhook_url:
    os.environ['SLACK_WEBHOOK_URL'] = args.slack_webhook_url

if args.admin_pass:
    os.environ['ADMIN_PASSWORD'] = args.admin_pass

if args.analyst_pass:
    os.environ['ANALYST_PASSWORD'] = args.analyst_pass

# Install chromedriver and add it to PATH
chromedriver_filename = 'chromedriver.exe' if platform.system() == "Windows" else 'chromedriver'
pyderman.install(file_directory='./webdriver/',
                 filename=chromedriver_filename,
                 verbose=True,
                 chmod=True,
                 overwrite=False,
                 version=args.chromedriver_version)

os.environ["PATH"] += os.pathsep + str(Path('webdriver').absolute())

output_file = "rerun.xml" if args.rerun_failed_tests or args.rerun_failed_suites else "output.xml"

# Set robotArgs
robotArgs = ["--outputdir", "test-results/",
             "--output", output_file,
             "--exclude", "Failing",
             "--exclude", "UnderConstruction",
             "--exclude", "BootstrapData"]

robotArgs += ["-v", f"timeout:{args.timeout}", "-v", f"implicit_wait:{args.implicit_wait}"]

if args.fail_fast:
    robotArgs += ["--exitonfailure"]

if args.rerun_failed_tests:
    robotArgs += ["--rerunfailed", "test-results/output.xml"]

if args.rerun_failed_suites:
    robotArgs += ["--rerunfailedsuites", "test-results/output.xml"]

if args.tags:
    robotArgs += ["--include", args.tags]

if args.print_keywords:
    robotArgs += ["--listener", 'listeners/KeywordListener.py']

if args.ci:
    robotArgs += ["--xunit", "xunit"]
    # NOTE(mark): Ensure secrets aren't visible in CI logs/reports
    robotArgs += ["--removekeywords", "name:operatingsystem.environment variable should be set"]
    robotArgs += ['--removekeywords',
                  'name:common.user goes to url']  # To hide basic auth credentials
else:
    if args.custom_env:
        load_dotenv(args.custom_env)

    else:
        load_dotenv('.env.' + args.env)

assert os.getenv('PUBLIC_URL') is not None
assert os.getenv('ADMIN_URL') is not None
assert os.getenv('ADMIN_EMAIL') is not None
assert os.getenv('ADMIN_PASSWORD') is not None


# seed Azure storage emulator release files
if (args.env == 'local'):
    generator = ReleaseFilesGenerator()
    generator.create_public_release_files()
    generator.create_private_release_files()


def admin_request(method, endpoint, body=None):
    assert method and endpoint
    assert os.getenv('ADMIN_URL') is not None
    assert os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN') is not None

    if method == 'POST':
        assert body is not None, 'POST requests require a body'

    requests.sessions.HTTPAdapter(
        pool_connections=50,
        pool_maxsize=50,
        max_retries=3
    )
    session = requests.Session()

    # To prevent InsecureRequestWarning
    requests.packages.urllib3.disable_warnings()

    jwt_token = json.loads(os.getenv('IDENTITY_LOCAL_STORAGE_ADMIN'))['access_token']
    headers = {
        'Content-Type': 'application/json',
        'Authorization': f'Bearer {jwt_token}',
    }
    response = session.request(
        method,
        url=f'{os.getenv("ADMIN_URL")}{endpoint}',
        headers=headers,
        stream=True,
        json=body,
        verify=False
    )

    if response.status_code in {401, 403}:
        print('Attempting re-authentication...', flush=True)

        # Delete identify files and re-attempt to fetch them
        setup_authentication(clear_existing=True)
        jwt_token = json.loads(os.environ['IDENTITY_LOCAL_STORAGE_ADMIN'])['access_token']
        response = session.request(
            method,
            url=f'{os.getenv("ADMIN_URL")}{endpoint}',
            headers={
                'Content-Type': 'application/json',
                'Authorization': f'Bearer {jwt_token}',
            },
            stream=True,
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
                  '--exclude', 'NotAgainstTest',
                  '--exclude', 'AltersData']
if args.env == 'preprod':
    robotArgs += ['--include', 'Preprod',
                  '--exclude', 'AltersData',
                  '--exclude', 'NotAgainstPreProd']

if args.env == 'prod':
    robotArgs += ['--include', 'Prod',
                  '--exclude', 'AltersData',
                  '--exclude', 'NotAgainstProd']

if args.visual:
    robotArgs += ["-v", "headless:0"]
else:
    robotArgs += ["-v", "headless:1"]

if os.getenv('RELEASE_COMPLETE_WAIT'):
    robotArgs += ["-v", f"release_complete_wait:{os.getenv('RELEASE_COMPLETE_WAIT')}"]

if os.getenv('FAIL_TEST_SUITES_FAST'):
    robotArgs += ["-v", f"FAIL_TEST_SUITES_FAST:{os.getenv('FAIL_TEST_SUITES_FAST')}"]

if args.prompt_to_continue:
    robotArgs += ["-v", "prompt_to_continue_on_failure:1"]

robotArgs += ["-v", "browser:" + args.browser]
robotArgs += [args.tests]

# Remove any existing test results if running from scratch
if not args.rerun_failed_tests and not args.rerun_failed_suites and Path('test-results').exists():
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
            kp.run_keyword_profile(f'test-results/{output_file}',
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
            kp.run_keyword_profile(f'test-results/{output_file}',
                                   printresults=False,
                                   writepath='test-results/keyword-profiling-results.log')
            print("\nProfiling logs created in test-results/", flush=True)
        else:
            pabot_run_cli(robotArgs)
finally:
    if not args.disable_teardown:
        print("Tearing down tests...", flush=True)
        delete_test_topic()

    if args.rerun_failed_tests or args.rerun_failed_suites:
        print("Combining rerun test results with original test results")
        merge_options = [
            "--outputdir", "test-results/",
            "-o", "output.xml",
            "--prerebotmodifier", "report-modifiers/CheckForAtLeastOnePassingRunPrerebotModifier.py",
            "--merge", "test-results/output.xml", "test-results/rerun.xml"
        ]
        robot_rebot_cli(merge_options, exit=False)

    print(f"\nLog available at: file://{os.getcwd()}{os.sep}test-results{os.sep}log.html")
    print(f"Report available at: file://{os.getcwd()}{os.sep}test-results{os.sep}report.html")
    print("\nTests finished!")
    if args.enable_slack:
        send_slack_report(args.env)
