#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Run 'python run_tests.py -h' to see argument options
"""

import argparse
import datetime
import os
import random
import shutil
import string
from pathlib import Path
from zipfile import ZipFile

import admin_api as admin_api
import args_and_variables as args_and_variables
import tests.libs.selenium_elements as selenium_elements
from pabot.pabot import main_program as pabot_run_cli
from robot import rebot_cli as robot_rebot_cli
from robot import run_cli as robot_run_cli
from scripts.get_webdriver import get_webdriver
from tests.libs.create_emulator_release_files import ReleaseFilesGenerator
from tests.libs.fail_fast import failing_suites_filename, get_failing_test_suites
from tests.libs.logger import get_logger
from tests.libs.slack import SlackService

pabot_suite_names_filename = ".pabotsuitenames"
results_foldername = "test-results"
seed_data_files_filepath = "tests/files/seed-data-files.zip"
unzipped_seed_data_folderpath = "tests/files/.unzipped-seed-data-files"

logger = get_logger(__name__)


def setup_python_path():
    # This is super awkward but we have to explicitly
    # add the current directory to PYTHONPATH otherwise
    # the subprocesses started by pabot will not be able
    # to locate lib modules correctly for some reason.
    pythonpath = os.getenv("PYTHONPATH")
    if pythonpath:
        os.environ["PYTHONPATH"] += f":{str(current_dir)}"
    else:
        os.environ["PYTHONPATH"] = str(current_dir)


def unzip_data_files():
    if not os.path.exists(seed_data_files_filepath):
        logger.warn(f"Unable to find seed data files bundle at {seed_data_files_filepath}")
    else:
        with ZipFile(seed_data_files_filepath, "r") as zipfile:
            zipfile.extractall(unzipped_seed_data_folderpath)


def install_chromedriver(chromedriver_version: str):
    # Install chromedriver and add it to PATH
    get_webdriver(chromedriver_version)


def create_robot_arguments(arguments: argparse.Namespace, rerunning_failed: bool) -> []:
    robot_args = [
        "--outputdir",
        f"{results_foldername}/",
        "--exclude",
        "Failing",
        "--exclude",
        "UnderConstruction",
        "--exclude",
        "VisualTesting",
        "--xunit",
        "xunit",
    ]
    robot_args += ["-v", f"timeout:{os.getenv('TIMEOUT')}", "-v", f"implicit_wait:{os.getenv('IMPLICIT_WAIT')}"]
    if arguments.fail_fast:
        robot_args += ["--exitonfailure"]
    if arguments.tags:
        robot_args += ["--include", arguments.tags]
    if arguments.print_keywords:
        robot_args += ["--listener", "listeners/KeywordListener.py"]
    if arguments.ci:
        # NOTE(mark): Ensure secrets aren't visible in CI logs/reports
        robot_args += ["--removekeywords", "name:operatingsystem.environment variable should be set"]
        robot_args += ["--removekeywords", "name:common.user goes to url"]  # To hide basic auth credentials
    if arguments.reseed:
        robot_args += ["--include", "SeedDataGeneration"]
    else:
        robot_args += ["--exclude", "SeedDataGeneration"]
    if arguments.env == "local":
        robot_args += ["--include", "Local", "--exclude", "NotAgainstLocal"]
        # seed Azure storage emulator release files
        generator = ReleaseFilesGenerator()
        generator.create_public_release_files()
        generator.create_private_release_files()
    if arguments.env == "dev":
        robot_args += ["--include", "Dev", "--exclude", "NotAgainstDev"]
    if arguments.env == "test":
        robot_args += ["--include", "Test", "--exclude", "NotAgainstTest", "--exclude", "AltersData"]
    # fmt off
    if arguments.env == "preprod":
        robot_args += ["--include", "Preprod", "--exclude", "AltersData", "--exclude", "NotAgainstPreProd"]
    # fmt on
    if arguments.env == "prod":
        robot_args += ["--include", "Prod", "--exclude", "AltersData", "--exclude", "NotAgainstProd"]
    if arguments.visual:
        robot_args += ["-v", "headless:0"]
    else:
        robot_args += ["-v", "headless:1"]
    if os.getenv("RELEASE_COMPLETE_WAIT"):
        robot_args += ["-v", f"release_complete_wait:{os.getenv('RELEASE_COMPLETE_WAIT')}"]
    if os.getenv("FAIL_TEST_SUITES_FAST"):
        robot_args += ["-v", f"FAIL_TEST_SUITES_FAST:{os.getenv('FAIL_TEST_SUITES_FAST')}"]
    if arguments.prompt_to_continue:
        robot_args += ["-v", "prompt_to_continue_on_failure:1"]
    if arguments.debug:
        robot_args += ["--loglevel", "DEBUG"]
    robot_args += ["-v", "browser:" + arguments.browser]
    # We want to add arguments on the first rerun attempt, but on subsequent attempts, we just want
    # to change rerunfailedsuites xml file we use
    if rerunning_failed:
        robot_args += ["--rerunfailedsuites", f"{results_foldername}/output.xml", "--output", "rerun.xml"]
    else:
        robot_args += ["--output", "output.xml"]

    robot_args += [arguments.tests]

    return robot_args


def create_run_identifier():
    # Add randomness to prevent multiple simultaneous run_tests.py generating the same run_identifier value
    random_str = "".join([random.choice(string.ascii_lowercase + string.digits) for n in range(6)])
    return datetime.datetime.utcnow().strftime("%Y%m%d-%H%M%S-" + random_str)


def merge_test_reports():
    merge_args = [
        "--outputdir",
        f"{results_foldername}/",
        "-o",
        "output.xml",
        "--xunit",
        "xunit.xml",
        "--prerebotmodifier",
        "report-modifiers/CheckForAtLeastOnePassingRunPrerebotModifier.py",
        "--merge",
        f"{results_foldername}/output.xml",
        f"{results_foldername}/rerun.xml",
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


def execute_tests(arguments: argparse.Namespace, rerunning_failures: bool):
    if arguments.interp == "robot":
        robot_run_cli(create_robot_arguments(arguments, rerunning_failures), exit=False)
    elif arguments.interp == "pabot":
        pabot_run_cli(create_robot_arguments(arguments, rerunning_failures))


def run():
    # Unzip any data files that may be used in tests.
    unzip_data_files()

    args = args_and_variables.initialise()

    # If running all tests, or admin, admin_and_public, admin_and_public_2 or seed_data suites,
    # these change data on environments and require test themes, test topics and user authentication.
    #
    # We check for both explicit forward slashes AND OS-specific separators here, as in Windows
    # we can expect to get either scenario depending upon how we're running these tests e.g.
    # Git Bash versus Command Prompt versus IDEs.  {os.sep} in Windows always evaluates to
    # `\\` whereas any of these run options above may choose to either supply forward slashes or
    # back slashes.
    data_changing_tests = (
        args.tests == "tests/"
        or args.tests == f"tests{os.sep}"
        or f"{os.sep}admin" in args.tests
        or "/admin" in args.tests
        or f"{os.sep}seed_data" in args.tests
        or "/seed_data" in args.tests
    )

    if data_changing_tests and args.env not in ["local", "dev"]:
        raise Exception(f"Cannot run tests that change data on environment {args.env}")

    install_chromedriver(args.chromedriver_version)

    test_run_index = -1

    logger.info(f"Running Robot tests with {args.rerun_attempts} rerun attempts for any failing suites")

    try:
        # Run tests
        while args.rerun_attempts is None or test_run_index < args.rerun_attempts:
            try:
                test_run_index += 1

                # Ensure all SeleniumLibrary elements and keywords are updated to use a branch new
                # Selenium instance for every test (re)run.
                if test_run_index > 0:
                    selenium_elements.clear_instances()

                rerunning_failed_suites = args.rerun_failed_suites or test_run_index > 0

                # Perform any cleanup before the test run.
                clear_files_before_test_run(rerunning_failed_suites)

                if not Path(f"{results_foldername}/downloads").exists():
                    os.makedirs(f"{results_foldername}/downloads")

                # Create a unique run identifier so that this test run's data will be unique.
                run_identifier = create_run_identifier()
                os.environ["RUN_IDENTIFIER"] = run_identifier

                # Create a Test Topic under which all of this test run's data will be created.
                if data_changing_tests:
                    admin_api.create_test_topic(run_identifier)

                # Run the tests.
                logger.info(f"Performing test run {test_run_index + 1} with unique identifier {run_identifier}")
                execute_tests(args, rerunning_failed_suites)

                # If we're rerunning failures, merge the former run's results with this run's
                # results.
                if rerunning_failed_suites:
                    logger.info(f"Merging results from test run {test_run_index + 1} with previous run's report")
                    merge_test_reports()

            finally:
                # Tear down any data created by this test run unless we've disabled teardown.
                if data_changing_tests and not args.disable_teardown:
                    logger.info("Tearing down test data...")
                    admin_api.delete_test_topic()

            # If all tests passed, return early.
            if not get_failing_test_suites():
                break

        logger.info(f"Log available at: file://{os.getcwd()}{os.sep}{results_foldername}{os.sep}log.html")
        logger.info(f"Report available at: file://{os.getcwd()}{os.sep}{results_foldername}{os.sep}report.html")

        logger.info(f"Number of test runs: {test_run_index + 1}")

        failing_suites = get_failing_test_suites()

        if failing_suites:
            logger.info(f"Number of failing suites: {len(failing_suites)}")
            logger.info(f"Failing suites:")
            [logger.info(r"  * file://" + suite.strip()) for suite in failing_suites]
        else:
            logger.info("\nAll tests passed!")

        if args.enable_slack_notifications:
            slack_service = SlackService()
            slack_service.send_test_report(args.env, args.tests, failing_suites, test_run_index)

    except Exception as ex:
        if args.enable_slack_notifications:
            slack_service = SlackService()
            slack_service.send_exception_details(args.env, args.tests, test_run_index, ex)
        raise ex


current_dir = Path(__file__).absolute().parent
os.chdir(current_dir)

setup_python_path()

# Run the tests!
run()
