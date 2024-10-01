#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Run 'python run_tests.py -h' to see argument options
"""

import datetime
import glob
import os
import random
import shutil
import string
import time
from pathlib import Path
from zipfile import ZipFile

import admin_api as admin_api
import args_and_variables as args_and_variables
import tests.libs.selenium_elements as selenium_elements
from scripts.get_webdriver import get_webdriver
from tests.libs.create_emulator_release_files import ReleaseFilesGenerator
from tests.libs.fail_fast import failing_suites_filename, get_failing_test_suites
from tests.libs.logger import get_logger
from tests.libs.slack import SlackService
import reports
import test_runners

pabot_suite_names_filename = ".pabotsuitenames"
main_results_folder = "test-results"
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


def create_run_identifier():
    # Add randomness to prevent multiple simultaneous run_tests.py generating the same run_identifier value
    random_str = "".join([random.choice(string.ascii_lowercase + string.digits) for n in range(6)])
    return datetime.datetime.utcnow().strftime("%Y%m%d-%H%M%S-" + random_str)


def clear_files_before_test_run(rerunning_failures: bool):
    # Remove any existing test results if running from scratch. Leave in place if re-running failures
    # as we'll need the old results to merge in with the rerun results.
    if not rerunning_failures and Path(main_results_folder).exists():
        shutil.rmtree(main_results_folder)

    # Remove any prior failing suites so the new test run is not marking any running test suites as
    # failed already.
    if Path(failing_suites_filename).exists():
        os.remove(failing_suites_filename)

    # If running with Pabot, remove any existing Pabot suites file as a pre-existing one can otherwise
    # cause single suites to run multiple times in parallel.
    if Path(pabot_suite_names_filename).exists():
        os.remove(pabot_suite_names_filename)


def run():

    robot_tests_folder = Path(__file__).absolute().parent
    
    args = args_and_variables.initialise()

    # Unzip any data files that may be used in tests.
    unzip_data_files()

    # Upload test data files to storage if running locally.
    if args.env == "local":
        files_generator = ReleaseFilesGenerator()
        files_generator.create_public_release_files()
        files_generator.create_private_release_files()

    install_chromedriver(args.chromedriver_version)

    test_run_index = -1
    run_identifier_initial_value = create_run_identifier()

    logger.info(f"Running Robot tests with {args.rerun_attempts} rerun attempts for any failing suites")

    if args.rerun_failed_suites: 
        logger.info(f"Clearing old run folders prior to rerunning failed tests")
        for old_run_folders in glob.glob(rf"{main_results_folder}{os.sep}run-*"):
            shutil.rmtree(old_run_folders)
            
    try:
        # Run tests
        while args.rerun_attempts is None or test_run_index < args.rerun_attempts:
            try:
                test_run_index += 1

                # Ensure all SeleniumLibrary elements and keywords are updated to use a brand new
                # Selenium instance for every test (re)run.
                if test_run_index > 0:
                    selenium_elements.clear_instances()

                rerunning_failed_suites = args.rerun_failed_suites or test_run_index > 0
                
                # Perform any cleanup before the test run.
                clear_files_before_test_run(rerunning_failed_suites)

                # Create a folder to contain this test run attempt's outputs and reports.
                test_run_results_folder = f"{main_results_folder}{os.sep}run-{test_run_index + 1}"
                os.makedirs(test_run_results_folder)

                if not Path(f"{main_results_folder}/downloads").exists():
                    os.makedirs(f"{main_results_folder}/downloads")

                # Create a unique run identifier so that this test run's data will be unique.
                run_identifier = f"{run_identifier_initial_value}-{test_run_index}"
                os.environ["RUN_IDENTIFIER"] = run_identifier

                # Create a Test Topic under which all of this test run's data will be created.
                if args_and_variables.includes_data_changing_tests(args):
                    admin_api.create_test_topic(run_identifier)

                # If re-running failed suites, get the appropriate previous report file from which to determine which suites failed.
                # If this is run 2 or more when using the `--rerun-attempts n` option, the path to the previous report will be in the
                # previous run attempt's folder.
                # If this is a rerun using the `--rerun-failed-suites` option, the path to the previous report will be in the main 
                # test results folder directly.
                if rerunning_failed_suites:
                    if test_run_index == 0:
                        previous_report_file = f"{robot_tests_folder}{os.sep}{main_results_folder}{os.sep}output.xml"
                    else:
                        previous_report_file = f"{robot_tests_folder}{os.sep}{main_results_folder}{os.sep}run-{test_run_index}{os.sep}output.xml"
                else:
                    previous_report_file = None

                # Run the tests.
                logger.info(f"Performing test run {test_run_index + 1} with unique identifier {run_identifier}")
                test_runners.execute_tests(args, test_run_results_folder, previous_report_file)
                
                if test_run_index > 0:
                    reports.create_report_from_output_xml(test_run_results_folder)
                
            finally:
                # Tear down any data created by this test run unless we've disabled teardown.
                if args_and_variables.includes_data_changing_tests(args) and not args.disable_teardown:
                    logger.info("Tearing down test data...")
                    admin_api.delete_test_topic()

            # If all tests passed, return early.
            if not get_failing_test_suites():
                break

        failing_suites = get_failing_test_suites()

        # Merge together all reports from all test runs.
        number_of_test_runs = test_run_index + 1
        reports.merge_robot_reports(number_of_test_runs)

        # Log the results of the merge test runs.
        reports.log_report_results(number_of_test_runs, failing_suites)

        if args.enable_slack_notifications:
            slack_service = SlackService()
            # Wait for 5 seconds to ensure the merge reports are properly synchronized after rerun attempts.
            time.sleep(5)
            slack_service.send_test_report(args.env, args.tests, failing_suites, number_of_test_runs)

    except Exception as ex:
        if args.enable_slack_notifications:
            slack_service = SlackService()
            slack_service.send_exception_details(args.env, args.tests, number_of_test_runs, ex)
        raise ex


current_dir = Path(__file__).absolute().parent
os.chdir(current_dir)

setup_python_path()

# Run the tests!
run()
