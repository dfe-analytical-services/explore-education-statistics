#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Run 'python run_tests.py -h' to see argument options
"""

import argparse
import glob
import os
import random
import shutil
import string
import sys
import time
from pathlib import Path
from zipfile import ZipFile

import admin_api as admin_api
import args_and_variables as args_and_variables
import reports
import test_runners
import tests.libs.selenium_elements as selenium_elements
from scripts.get_webdriver import get_webdriver
from tests.libs.create_emulator_release_files import ReleaseFilesGenerator
from tests.libs.fail_fast import failing_suites_filename
from tests.libs.logger import get_logger
from tests.libs.slack import SlackService

pabot_suite_names_filename = ".pabotsuitenames"
main_results_folder = "test-results"
seed_data_files_filepath = "tests/files/seed-data-files.zip"
unzipped_seed_data_folderpath = "tests/files/.unzipped-seed-data-files"

logger = get_logger(__name__)


def _setup_python_path():
    # This is super awkward but we have to explicitly
    # add the current directory to PYTHONPATH otherwise
    # the subprocesses started by pabot will not be able
    # to locate lib modules correctly for some reason.
    pythonpath = os.getenv("PYTHONPATH")
    if pythonpath:
        os.environ["PYTHONPATH"] += f":{str(current_dir)}"
    else:
        os.environ["PYTHONPATH"] = str(current_dir)


def _unzip_data_files():
    if not os.path.exists(seed_data_files_filepath):
        logger.warning(f"Unable to find seed data files bundle at {seed_data_files_filepath}")
    else:
        with ZipFile(seed_data_files_filepath, "r") as zipfile:
            zipfile.extractall(unzipped_seed_data_folderpath)


def _install_chromedriver(chromedriver_version: str):
    # Install chromedriver and add it to PATH
    get_webdriver(chromedriver_version)


def _generate_random_id(size=6):
    return "".join(random.choices(string.ascii_lowercase + string.digits, k=size))


def _clear_files_before_next_test_run_attempt(rerunning_failures: bool):
    # Remove any prior failing suites so the new test run is not marking any running test suites as
    # failed already.
    if Path(failing_suites_filename).exists():
        os.remove(failing_suites_filename)

    # If running with Pabot, remove any existing Pabot suites file as a pre-existing one can otherwise
    # cause single suites to run multiple times in parallel.
    if Path(pabot_suite_names_filename).exists():
        os.remove(pabot_suite_names_filename)


def _setup_main_results_folder_for_first_run(args: argparse.Namespace):
    # If rerunning failing tests from a previous execution of run_tests.py, remove any existing "run-x" test run folders
    # and copy the main results folder contents into a new "run-0" folder to represent the previous run. Start the first
    # run of this rerun as "run-1".
    if args.rerun_failed_suites:
        previous_report_file_path = f"{main_results_folder}{os.sep}output.xml"
        if not os.path.exists(previous_report_file_path):
            logger.error(
                f"No previous report file found at {previous_report_file_path} - unable to rerun failed suites"
            )
            sys.exit(1)
        logger.info(f"Clearing old run folders prior to rerunning failed tests")
        for old_run_folders in glob.glob(rf"{main_results_folder}{os.sep}run-*"):
            shutil.rmtree(old_run_folders)
        test_run_1_folder = f"{main_results_folder}{os.sep}run-0"
        logger.info(f'Copying previous test results into new "{test_run_1_folder}" folder')
        shutil.copytree(main_results_folder, test_run_1_folder)
    else:
        # Remove any existing test results if running from scratch.
        if Path(main_results_folder).exists():
            shutil.rmtree(main_results_folder)
            os.mkdir(main_results_folder)


def run():
    args = args_and_variables.initialise()

    _setup_main_results_folder_for_first_run(args)

    # Unzip any data files that may be used in tests.
    _unzip_data_files()

    # Upload test data files to storage if running locally.
    if args.env == "local":
        files_generator = ReleaseFilesGenerator()
        files_generator.create_public_release_files()
        files_generator.create_private_release_files()

    _install_chromedriver(args.chromedriver_version)

    run_identifier_initial_value = _generate_random_id()

    max_run_attempts = args.rerun_attempts + 1
    test_run_index = 0

    logger.info(f"Running Robot tests with {max_run_attempts} maximum run attempts")

    try:
        # Run tests
        while test_run_index < max_run_attempts:
            try:
                test_run_results_folder = f"{main_results_folder}{os.sep}run-{test_run_index + 1}"

                # Ensure all SeleniumLibrary elements and keywords are updated to use a brand new
                # Selenium instance for every test (re)run.
                if test_run_index > 0:
                    selenium_elements.clear_instances()

                rerunning_failed_suites = args.rerun_failed_suites or test_run_index > 0

                # Perform any cleanup before the test run.
                _clear_files_before_next_test_run_attempt(rerunning_failed_suites)

                # Create a folder to contain this test run attempt's outputs and reports.
                os.makedirs(test_run_results_folder)

                if not Path(f"{main_results_folder}/downloads").exists():
                    os.makedirs(f"{main_results_folder}/downloads")

                # Create a unique run identifier so that this test run's data will be unique.
                run_identifier = f"{run_identifier_initial_value}-{test_run_index}"
                os.environ["RUN_IDENTIFIER"] = run_identifier

                # Create a Test Theme under which all of this test run's data will be created.
                if args_and_variables.includes_data_changing_tests(args):
                    admin_api.create_test_theme(run_identifier)

                # If re-running failed suites, get the appropriate report file from the previous "run-x" folder. This will contain details of
                # any failed tests from the previous run.
                if rerunning_failed_suites:
                    previous_report_file = f"{main_results_folder}{os.sep}run-{test_run_index}{os.sep}output.xml"
                    logger.info(
                        f'Using previous test run\'s results file "{previous_report_file}" to determine which failing suites to run'
                    )
                else:
                    previous_report_file = None

                # Run the tests.
                logger.info(
                    f'Performing test run {test_run_index + 1} in test run folder "{test_run_results_folder}" with unique identifier {run_identifier}'
                )
                test_runners.execute_tests(args, test_run_results_folder, previous_report_file)

                if test_run_index > 0:
                    reports.create_report_from_output_xml(test_run_results_folder)

            finally:
                # Tear down any data created by this test run unless we've disabled teardown.
                if args_and_variables.includes_data_changing_tests(args) and not args.disable_teardown:
                    logger.info("Tearing down test data...")
                    admin_api.delete_test_theme()

                test_run_index += 1

            failing_suites = reports.get_failing_test_suite_sources(f"{test_run_results_folder}{os.sep}output.xml")

            # If all tests passed, return early.
            if len(failing_suites) == 0:
                break

        # Merge together all reports from all test runs.
        number_of_test_runs = test_run_index
        first_run_folder_number = 0 if args.rerun_failed_suites else 1

        reports.merge_robot_reports(first_run_folder_number, number_of_test_runs)

        # Log the results of the merge test runs.
        reports.log_report_results(number_of_test_runs, rerunning_failed_suites, failing_suites)

        if args.enable_slack_notifications:
            slack_service = SlackService()
            # Wait for 5 seconds to ensure the merge reports are properly synchronized after rerun attempts.
            time.sleep(5)
            slack_service.send_test_report(args.env, args.tests, failing_suites, number_of_test_runs)

        if len(failing_suites) > 0:
            sys.exit(1)

    except Exception as ex:
        if args.enable_slack_notifications:
            slack_service = SlackService()
            slack_service.send_exception_details(args.env, args.tests, number_of_test_runs, ex)
        raise ex


current_dir = Path(__file__).absolute().parent
os.chdir(current_dir)

_setup_python_path()

# Run the tests!
run()
