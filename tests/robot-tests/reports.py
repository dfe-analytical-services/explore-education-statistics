import os
import glob
import shutil
from bs4 import BeautifulSoup
from robot import rebot_cli as robot_rebot_cli
from tests.libs.logger import get_logger

main_results_folder = "test-results"

logger = get_logger(__name__)


# Merge multiple Robot test reports and assets together into the main test results folder.
def merge_robot_reports(number_of_test_runs: int):

    run_1_folder=f"{main_results_folder}{os.sep}run-1"

    for file in os.listdir(run_1_folder):
        _copy_to_destination_folder(run_1_folder, file, main_results_folder)

    for test_run in range(2, number_of_test_runs + 1):
        
        logger.info(f"Merging test run {test_run} results into full results")
        
        test_run_foldername = f"{main_results_folder}{os.sep}run-{test_run}"
        _merge_test_reports(test_run_foldername)
        
        for file in glob.glob(rf"{test_run_foldername}{os.sep}*screenshot*"):
            _copy_to_destination_folder(test_run_foldername, file.split(os.sep)[-1], main_results_folder)

        for file in glob.glob(rf"{test_run_foldername}{os.sep}*capture*"):
            _copy_to_destination_folder(test_run_foldername, file.split(os.sep)[-1], main_results_folder)


def log_report_results(number_of_test_runs: int, failing_suites: []):
    logger.info(f"Log available at: file://{os.getcwd()}{os.sep}{main_results_folder}{os.sep}log.html")
    logger.info(f"Report available at: file://{os.getcwd()}{os.sep}{main_results_folder}{os.sep}report.html")
    logger.info(f"Number of test runs: {number_of_test_runs}")

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
        f"{test_results_folder}/output.xml"
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
    
    with open(path_to_original_report, "rb") as report:
        
        report_contents = report.read()
        report = BeautifulSoup(report_contents, features="xml")
        
        passing_suite_ids = _get_passing_suite_ids_from_report(report)
        
        suite_elements = report.find_all('suite', recursive=True)
        [suite_element.extract() for suite_element in suite_elements if suite_element.get('id') in passing_suite_ids]
        
        suite_stats = report.find_all('stat', recursive=True)
        [suite_stat.extract() for suite_stat in suite_stats if suite_stat.get('id') in passing_suite_ids]
    
        with open(path_to_filtered_report, "a") as filtered_file:
            filtered_file.write(report.prettify())

        
def _get_passing_suite_ids_from_report(report: BeautifulSoup) -> []: 
    suite_results = report.find('statistics').find('suite').find_all('stat')
    passing_suite_results = [suite_result for suite_result in suite_results if int(suite_result.get('fail')) == 0]
    return [passing_suite.get('id') for passing_suite in passing_suite_results]


def _copy_to_destination_folder(source_folder: str, source_file: str, destination_folder: str):
    source_file_path=f"{source_folder}{os.sep}{source_file}"
    destination_file_path=f"{destination_folder}{os.sep}{source_file}"
    if os.path.isfile(source_file_path):
        shutil.copy(source_file_path, destination_file_path)
    else:
        shutil.copytree(source_file_path, destination_file_path, dirs_exist_ok=True)
