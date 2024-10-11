import glob
import os
import shutil

from bs4 import BeautifulSoup
from robot import rebot_cli as robot_rebot_cli
from tests.libs.logger import get_logger

main_results_folder = "test-results"

logger = get_logger(__name__)


# Merge multiple Robot test reports and assets together into the main test results folder.
def merge_robot_reports(first_run_attempt_number: int, number_of_test_runs: int):
    first_run_folder = f"{main_results_folder}{os.sep}run-{first_run_attempt_number}"

    logger.info(f"Merging test run {first_run_attempt_number} results into full results")

    for file in os.listdir(first_run_folder):
        _copy_to_destination_folder(first_run_folder, file, main_results_folder)

    for test_run in range(first_run_attempt_number + 1, number_of_test_runs + 1):
        logger.info(f"Merging test run {test_run} results into full results")

        test_run_foldername = f"{main_results_folder}{os.sep}run-{test_run}"
        _merge_test_reports(test_run_foldername)

        for file in glob.glob(rf"{test_run_foldername}{os.sep}*screenshot*"):
            _copy_to_destination_folder(test_run_foldername, file.split(os.sep)[-1], main_results_folder)

        for file in glob.glob(rf"{test_run_foldername}{os.sep}*capture*"):
            _copy_to_destination_folder(test_run_foldername, file.split(os.sep)[-1], main_results_folder)


def get_failing_test_suite_sources(path_to_report_file: str) -> []:
    with open(path_to_report_file, "rb") as report_file:
        report_contents = report_file.read()
        report = BeautifulSoup(report_contents, features="xml")

        failing_suite_ids = _get_failing_leaf_suite_ids_from_report(report)
        suite_elements = report.find_all("suite", recursive=True)
        return [
            suite_element.get("source")
            for suite_element in suite_elements
            if suite_element.get("id") in failing_suite_ids
        ]


def log_report_results(number_of_test_runs: int, reran_failing_suites: bool, failing_suites: []):
    logger.info("*************************************")
    logger.info("FINAL REPORT")
    logger.info(f"Log available at: file://{os.getcwd()}{os.sep}{main_results_folder}{os.sep}log.html")
    logger.info(f"Report available at: file://{os.getcwd()}{os.sep}{main_results_folder}{os.sep}report.html")
    logger.info("*************************************\n")

    if reran_failing_suites:
        logger.info("*************************************")
        logger.info("LAST RUN ATTEMPT REPORT")
        logger.info(
            f"Log available at: file://{os.getcwd()}{os.sep}{main_results_folder}{os.sep}run-{number_of_test_runs}{os.sep}log.html"
        )
        logger.info(
            f"Report available at: file://{os.getcwd()}{os.sep}{main_results_folder}{os.sep}run-{number_of_test_runs}{os.sep}report.html"
        )
        logger.info("*************************************\n")

    logger.info(f"Number of test run attempts: {number_of_test_runs}")
    if failing_suites:
        logger.info(f"Number of failing suites: {len(failing_suites)}")
        logger.info(f"Failing suites:")
        [logger.info(r"  * file://" + suite) for suite in failing_suites]
    else:
        logger.info("\nAll tests passed!")


def create_report_from_output_xml(test_results_folder: str):
    create_args = [
        "--outputdir",
        f"{test_results_folder}/",
        "-o",
        "output.xml",
        "--xunit",
        "xunit.xml",
        f"{test_results_folder}/output.xml",
    ]
    robot_rebot_cli(create_args, exit=False)


def _merge_test_reports(test_results_folder):
    merge_args = [
        "--outputdir",
        f"{main_results_folder}/",
        "-o",
        "output.xml",
        "--xunit",
        "xunit.xml",
        "--prerebotmodifier",
        "report-modifiers/CheckForAtLeastOnePassingRunPrerebotModifier.py",
        "--merge",
        f"{main_results_folder}/output.xml",
        f"{test_results_folder}/output.xml",
    ]
    robot_rebot_cli(merge_args, exit=False)


def filter_out_passing_suites_from_report_file(path_to_original_report: str, path_to_filtered_report: str):
    with open(path_to_original_report, "rb") as report_file:
        report_contents = report_file.read()
        report = BeautifulSoup(report_contents, features="xml")

        passing_suite_ids = _get_passing_suite_ids_from_report(report)

        suite_elements = report.find_all("suite", recursive=True)
        [suite_element.extract() for suite_element in suite_elements if suite_element.get("id") in passing_suite_ids]

        suite_stats = report.find_all("stat", recursive=True)
        [suite_stat.extract() for suite_stat in suite_stats if suite_stat.get("id") in passing_suite_ids]

        if os.path.exists(path_to_filtered_report):
            os.remove(path_to_filtered_report)

        with open(path_to_filtered_report, "a") as filtered_file:
            filtered_file.write(report.prettify())


def _get_passing_suite_ids_from_report(report: BeautifulSoup) -> []:
    suite_results = report.find("statistics").find("suite").find_all("stat")
    passing_suite_results = [suite_result for suite_result in suite_results if int(suite_result.get("fail")) == 0]
    return [passing_suite.get("id") for passing_suite in passing_suite_results]


def _get_failing_leaf_suite_ids_from_report(report: BeautifulSoup) -> []:
    """Get the ids of failing suites from output.xml, filtering out ids from parent levels in the suite hierarchy.

    Given an example suite folder structure of "Top-level folder:Sub-folder:Leaf suite", the report contains stats
    for each level, with ids like "s1", "s1-1" and "s1-1-1". This code returns only the leaf ids, so "s1-1-1" in this
    example.
    """
    suite_results = report.find("statistics").find("suite").find_all("stat")
    failing_suite_results = [suite_result for suite_result in suite_results if int(suite_result.get("fail")) > 0]
    suite_ids = [failing_suite.get("id") for failing_suite in failing_suite_results]
    leaf_suite_ids = []
    for suite_id in suite_ids:
        other_suite_ids_containing_suite_id = [
            other_suite_id for other_suite_id in suite_ids if other_suite_id != suite_id and suite_id in other_suite_id
        ]
        if len(other_suite_ids_containing_suite_id) == 0:
            leaf_suite_ids += [suite_id]
    return leaf_suite_ids


def _copy_to_destination_folder(source_folder: str, source_file: str, destination_folder: str):
    source_file_path = f"{source_folder}{os.sep}{source_file}"
    destination_file_path = f"{destination_folder}{os.sep}{source_file}"
    if os.path.isfile(source_file_path):
        shutil.copy(source_file_path, destination_file_path)
    else:
        shutil.copytree(source_file_path, destination_file_path, dirs_exist_ok=True)
