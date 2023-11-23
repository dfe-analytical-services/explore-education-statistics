#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Run 'python run_tests.py -h' to see argument options
"""

import argparse
import datetime
import json
import os
import random
import shutil
import string
from pathlib import Path

import requests
from dotenv import load_dotenv
from pabot.pabot import main_program as pabot_run_cli
from robot import rebot_cli as robot_rebot_cli
from robot import run_cli as robot_run_cli
from scripts.get_webdriver import get_webdriver
from tests.libs.create_emulator_release_files import ReleaseFilesGenerator
from tests.libs.fail_fast import failing_suites_filename
from tests.libs.logger import get_logger
from tests.libs.setup_auth_variables import setup_auth_variables
from tests.libs.slack import SlackService

pabot_suite_names_filename = ".pabotsuitenames"
results_foldername = "test-results"

current_dir = Path(__file__).absolute().parent
os.chdir(current_dir)


logger = get_logger(__name__)

# This is super awkward but we have to explicitly
# add the current directory to PYTHONPATH otherwise
# the subprocesses started by pabot will not be able
# to locate lib modules correctly for some reason.
pythonpath = os.getenv("PYTHONPATH")

if pythonpath:
    os.environ["PYTHONPATH"] += f":{str(current_dir)}"
else:
    os.environ["PYTHONPATH"] = str(current_dir)


# Parse arguments
parser = argparse.ArgumentParser(
    prog="pipenv run python run_tests.py",
    description="Use this script to run the UI tests, locally or as part of the CI pipeline, against the environment of your choosing",
)
parser.add_argument(
    "-b",
    "--browser",
    dest="browser",
    default="chrome",
    choices=["chrome", "firefox", "ie"],
    help="name of the browser you wish to run the tests with (NOTE: Only chromedriver is automatically installed!)",
)
parser.add_argument(
    "-i",
    "--interp",
    dest="interp",
    default="pabot",
    choices=["pabot", "robot"],
    help="interpreter to use to run the tests",
)
parser.add_argument(
    "--processes", dest="processes", help="how many processes should be used when using the pabot interpreter"
)
parser.add_argument(
    "-e",
    "--env",
    dest="env",
    default="test",
    choices=["local", "dev", "test", "preprod", "prod", "ci"],
    help="the environment to run the tests against",
)
parser.add_argument(
    "-f",
    "--file",
    dest="tests",
    metavar="{file/dir}",
    default="tests/",
    help="test suite or folder of tests suites you wish to run",
)
parser.add_argument(
    "-t", "--tags", dest="tags", nargs="?", metavar="{tag(s)}", help="specify tests you wish to run by tag"
)
parser.add_argument(
    "-v", "--visual", dest="visual", action="store_true", help="display browser window that the tests run in"
)
parser.add_argument(
    "--ci", dest="ci", action="store_true", help="specify that the test are running as part of the CI pipeline"
)
parser.add_argument(
    "--chromedriver",
    dest="chromedriver_version",
    metavar="{version}",
    help="specify which version of chromedriver to use",
)
parser.add_argument(
    "--disable-teardown",
    dest="disable_teardown",
    help="disable tearing down of any test data after completion",
    action="store_true",
)
parser.add_argument(
    "--rerun-failed-tests",
    dest="rerun_failed_tests",
    action="store_true",
    help="rerun individual failed tests and merge results into original run results",
)
parser.add_argument(
    "--rerun-failed-suites",
    dest="rerun_failed_suites",
    action="store_true",
    help="rerun failed test suites and merge results into original run results",
)
parser.add_argument("--rerun-attempts", dest="rerun_attempts", type=int, default=0, help="Number of rerun attempts")
parser.add_argument(
    "--print-keywords",
    dest="print_keywords",
    action="store_true",
    help="choose to print out keywords as they are started",
)
parser.add_argument(
    "--enable-slack-notifications",
    dest="enable_slack_notifications",
    action="store_true",
    help="enable Slack notifications to be sent for test reports",
)
parser.add_argument(
    "--prompt-to-continue",
    dest="prompt_to_continue",
    action="store_true",
    help="get prompted to continue with test execution upon a failure",
)
parser.add_argument("--fail-fast", dest="fail_fast", action="store_true", help="stop test execution on failure")
parser.add_argument(
    "--custom-env", dest="custom_env", default=None, help="load a custom .env file (must be in ~/robot-tests directory)"
)
parser.add_argument(
    "--debug",
    dest="debug",
    action="store_true",
    help="get debug-level logging in report.html, including Python tracebacks",
)

"""
NOTE(mark): The admin and analyst passwords to access the Admin app are
stored in the CI pipeline as secret variables, which means they cannot be accessed as normal
environment variables, and instead must be passed as an argument to this script.
"""
parser.add_argument("--admin-pass", dest="admin_pass", default=None, help="manually specify the admin password")
parser.add_argument("--analyst-pass", dest="analyst_pass", default=None, help="manually specify the analyst password")
parser.add_argument(
    "--expiredinvite-pass",
    dest="expiredinvite_pass",
    default=None,
    help="manually specify the expiredinvite user password",
)
args = parser.parse_args()

if args.custom_env:
    load_dotenv(args.custom_env)
else:
    load_dotenv(".env." + args.env)


required_env_vars = [
    "TIMEOUT",
    "IMPLICIT_WAIT",
    "PUBLIC_URL",
    "ADMIN_URL",
    "PUBLIC_AUTH_USER",
    "PUBLIC_AUTH_PASSWORD",
    "RELEASE_COMPLETE_WAIT",
    "WAIT_MEDIUM",
    "WAIT_LONG",
    "WAIT_SMALL",
    "FAIL_TEST_SUITES_FAST",
    "IDENTITY_PROVIDER",
    "WAIT_CACHE_EXPIRY",
    "EXPIRED_INVITE_USER_EMAIL",
    "PUBLISHER_FUNCTIONS_URL",
]

for env_var in required_env_vars:
    assert os.getenv(env_var) is not None, f"Environment variable {env_var} is not set"

if args.admin_pass:
    os.environ["ADMIN_PASSWORD"] = args.admin_pass

if args.analyst_pass:
    os.environ["ANALYST_PASSWORD"] = args.analyst_pass

if args.expiredinvite_pass:
    os.environ["EXPIRED_INVITE_USER_PASSWORD"] = args.expiredinvite_pass

# Install chromedriver and add it to PATH
get_webdriver(args.chromedriver_version or None)


def admin_request(method, endpoint, body=None):
    assert method and endpoint
    assert os.getenv("ADMIN_URL") is not None
    assert os.getenv("IDENTITY_LOCAL_STORAGE_ADMIN") is not None

    if method == "POST":
        assert body is not None, "POST requests require a body"

    requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
    session = requests.Session()

    # To prevent InsecureRequestWarning
    requests.packages.urllib3.disable_warnings()

    jwt_token = json.loads(os.getenv("IDENTITY_LOCAL_STORAGE_ADMIN"))["access_token"]
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {jwt_token}",
    }
    response = session.request(
        method, url=f'{os.getenv("ADMIN_URL")}{endpoint}', headers=headers, stream=True, json=body, verify=False
    )

    if response.status_code in {401, 403}:
        logger.info("Attempting re-authentication...")

        # Delete identify files and re-attempt to fetch them
        setup_authentication(clear_existing=True)
        jwt_token = json.loads(os.environ["IDENTITY_LOCAL_STORAGE_ADMIN"])["access_token"]
        response = session.request(
            method,
            url=f'{os.getenv("ADMIN_URL")}{endpoint}',
            headers={
                "Content-Type": "application/json",
                "Authorization": f"Bearer {jwt_token}",
            },
            stream=True,
            json=body,
            verify=False,
        )

        assert response.status_code not in {401, 403}, "Failed to reauthenticate."

    if response.status_code == 400 and response.text.find("SlugNotUnique") != -1:
        raise Exception(f"SlugNotUnique for {body}")
    else:
        assert response.status_code < 300, f"Admin request responded with {response.status_code} and {response.text}"
    return response


def get_test_themes():
    return admin_request("GET", "/api/themes")


def create_test_theme():
    return admin_request("POST", "/api/themes", {"title": "Test theme", "summary": "Test theme summary"})


def create_test_topic(run_id: str):
    setup_authentication()

    if args.env in ["local", "dev"]:
        get_themes_resp = get_test_themes()
        test_theme_id = None
        test_theme_name = "Test theme"

        for theme in get_themes_resp.json():
            if theme["title"] == test_theme_name:
                test_theme_id = theme["id"]
                break
        if not test_theme_id:
            create_theme_resp = create_test_theme()
            test_theme_id = create_theme_resp.json()["id"]

        os.environ["TEST_THEME_NAME"] = test_theme_name
        os.environ["TEST_THEME_ID"] = test_theme_id

    assert os.getenv("TEST_THEME_ID") is not None

    topic_name = f"UI test topic {run_id}"
    resp = admin_request("POST", "/api/topics", {"title": topic_name, "themeId": os.getenv("TEST_THEME_ID")})

    os.environ["TEST_TOPIC_NAME"] = topic_name
    os.environ["TEST_TOPIC_ID"] = resp.json()["id"]


def delete_test_topic():
    if os.getenv("TEST_TOPIC_ID") is not None:
        admin_request("DELETE", f'/api/topics/{os.getenv("TEST_TOPIC_ID")}')


def setup_authentication(clear_existing=False):
    # Don't need BAU user if running general_public tests
    if "general_public" not in args.tests:
        setup_auth_variables(
            user="ADMIN",
            email=os.getenv("ADMIN_EMAIL"),
            password=os.getenv("ADMIN_PASSWORD"),
            clear_existing=clear_existing,
            identity_provider=os.getenv("IDENTITY_PROVIDER"),
        )

    # Don't need analyst user if running admin/bau or admin_and_public/bau tests
    if f"{os.sep}bau" not in args.tests:
        setup_auth_variables(
            user="ANALYST",
            email=os.getenv("ANALYST_EMAIL"),
            password=os.getenv("ANALYST_PASSWORD"),
            clear_existing=clear_existing,
            identity_provider=os.getenv("IDENTITY_PROVIDER"),
        )


def create_robot_arguments(rerunning_failed: bool) -> []:
    robot_args = [
        "--outputdir",
        "test-results/",
        "--exclude",
        "Failing",
        "--exclude",
        "UnderConstruction",
        "--exclude",
        "BootstrapData",
        "--exclude",
        "VisualTesting",
    ]
    robot_args += ["-v", f"timeout:{os.getenv('TIMEOUT')}", "-v", f"implicit_wait:{os.getenv('IMPLICIT_WAIT')}"]
    if args.fail_fast:
        robot_args += ["--exitonfailure"]
    if args.tags:
        robot_args += ["--include", args.tags]
    if args.print_keywords:
        robot_args += ["--listener", "listeners/KeywordListener.py"]
    if args.ci:
        robot_args += ["--xunit", "xunit"]
        # NOTE(mark): Ensure secrets aren't visible in CI logs/reports
        robot_args += ["--removekeywords", "name:operatingsystem.environment variable should be set"]
        robot_args += ["--removekeywords", "name:common.user goes to url"]  # To hide basic auth credentials
    if args.env == "local":
        robot_args += ["--include", "Local"]
        robot_args += ["--exclude", "NotAgainstLocal"]
        # seed Azure storage emulator release files
        generator = ReleaseFilesGenerator()
        generator.create_public_release_files()
        generator.create_private_release_files()
    if args.env == "dev":
        robot_args += ["--include", "Dev"]
        robot_args += ["--exclude", "NotAgainstDev"]
    if args.env == "test":
        robot_args += ["--include", "Test", "--exclude", "NotAgainstTest", "--exclude", "AltersData"]
    # fmt off
    if args.env == "preprod":
        robot_args += ["--include", "Preprod", "--exclude", "AltersData", "--exclude", "NotAgainstPreProd"]
    # fmt on
    if args.env == "prod":
        robot_args += ["--include", "Prod", "--exclude", "AltersData", "--exclude", "NotAgainstProd"]
    if args.visual:
        robot_args += ["-v", "headless:0"]
    else:
        robot_args += ["-v", "headless:1"]
    if os.getenv("RELEASE_COMPLETE_WAIT"):
        robot_args += ["-v", f"release_complete_wait:{os.getenv('RELEASE_COMPLETE_WAIT')}"]
    if os.getenv("FAIL_TEST_SUITES_FAST"):
        robot_args += ["-v", f"FAIL_TEST_SUITES_FAST:{os.getenv('FAIL_TEST_SUITES_FAST')}"]
    if args.prompt_to_continue:
        robot_args += ["-v", "prompt_to_continue_on_failure:1"]
    if args.debug:
        robot_args += ["--loglevel", "DEBUG"]
    robot_args += ["-v", "browser:" + args.browser]
    # We want to add arguments on the first rerun attempt, but on subsequent attempts, we just want
    # to change rerunfailedsuites xml file we use
    if rerunning_failed:
        robot_args += ["--rerunfailedsuites", f"test-results/output.xml", "--output", "rerun.xml"]
    else:
        robot_args += ["--output", "output.xml"]

    robot_args += [args.tests]

    return robot_args


def get_failing_suites() -> []:
    if Path(failing_suites_filename).exists():
        return open(failing_suites_filename, "r").readlines()
    return []


if not os.path.exists("test-results/downloads"):
    os.makedirs("test-results/downloads")


def create_run_identifier():
    # Add randomness to prevent multiple simultaneous run_tests.py generating the same run_identifier value
    random_str = "".join([random.choice(string.ascii_lowercase + string.digits) for n in range(6)])
    return datetime.datetime.utcnow().strftime("%Y%m%d-%H%M%S-" + random_str)


def merge_test_reports():
    merge_args = [
        "--outputdir",
        "test-results/",
        "-o",
        "output.xml",
        "--prerebotmodifier",
        "report-modifiers/CheckForAtLeastOnePassingRunPrerebotModifier.py",
        "--merge",
        "test-results/output.xml",
        "test-results/rerun.xml",
    ]
    robot_rebot_cli(merge_args, exit=False)


def clear_files_before_test_run(rerunning_failures: bool):
    # Remove any existing test results if running from scratch. Leave in place if re-running failures
    # as we'll need the old results to merge in with the rerun results.
    if not rerunning_failures and Path(results_foldername).exists():
        shutil.rmtree(results_foldername)

    # Remove any prior failing suites so the new test run is not marking any running test suites as
    # failed already.
    if Path(failing_suites_filename).exists():
        os.remove(failing_suites_filename)

    # If running with Pabot, remove any existing Pabot suites file as a pre-existing one can otherwise
    # cause single suites to run multiple times in parallel.
    if Path(pabot_suite_names_filename).exists():
        os.remove(pabot_suite_names_filename)


def run_tests(rerunning_failures: bool):
    logger.info(f"Starting tests with RUN_IDENTIFIER: {run_identifier}")
    if args.interp == "robot":
        robot_run_cli(create_robot_arguments(rerunning_failures), exit=False)
    elif args.interp == "pabot":
        pabot_run_cli(create_robot_arguments(rerunning_failures))


test_run_index = -1

try:
    # Run tests
    while args.rerun_attempts is None or test_run_index < args.rerun_attempts:
        test_run_index += 1

        rerunning_failed_suites = args.rerun_failed_suites or test_run_index > 0

        # Perform any cleanup before the test run.
        clear_files_before_test_run(rerunning_failed_suites)

        # Create a unique run identifier so that this test run's data will be unique.
        run_identifier = create_run_identifier()
        os.environ["RUN_IDENTIFIER"] = run_identifier

        # Create a Test Topic under which all of this test run's data will be created.
        needs_test_topic = args.tests and "general_public" not in args.tests

        if needs_test_topic:
            create_test_topic(run_identifier)

        # Run the tests.
        run_tests(rerunning_failed_suites)

        # If we're rerunning failures, merge the former run's results with this run's
        # results.
        if rerunning_failed_suites:
            merge_test_reports()

        # Tear down any data created by this test run unless we've disabled teardown.
        if needs_test_topic and not args.disable_teardown:
            logger.info("Tearing down tests...")
            delete_test_topic()

        # If all tests passed, return early.
        if not get_failing_suites():
            break

finally:
    logger.info(f"Log available at: file://{os.getcwd()}{os.sep}test-results{os.sep}log.html")
    logger.info(f"Report available at: file://{os.getcwd()}{os.sep}test-results{os.sep}report.html")

    logger.info(f"Number of test runs: {test_run_index + 1}")

    failing_suites = get_failing_suites()

    if failing_suites:
        logger.info(f"Failing suites:")
        [logger.info(r"  * file://" + suite) for suite in failing_suites]
    else:
        logger.info("\nAll tests passed!")

    if args.enable_slack_notifications:
        slack_service = SlackService()
        slack_service.send_test_report(args.env, args.tests, failing_suites, test_run_index)
