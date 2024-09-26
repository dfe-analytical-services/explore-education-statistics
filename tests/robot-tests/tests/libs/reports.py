import glob
import os
import shutil
from robot import rebot_cli as robot_rebot_cli
from tests.libs.logger import get_logger

results_foldername = "test-results"

logger = get_logger(__name__)


# Merge multiple Robot test reports and assets together into the main test results folder.
def merge_robot_reports(number_of_test_runs: int):

    run_1_folder=f"{results_foldername}{os.sep}run-1"

    for file in os.listdir(run_1_folder):
        _copy_to_destination_folder(run_1_folder, file, results_foldername)

    for test_run in range(2, number_of_test_runs + 1):
        
        logger.info(f"Merging test run {test_run} results into full results")
        
        test_run_foldername = f"{results_foldername}{os.sep}run-{test_run}"
        _merge_test_reports(test_run_foldername)
        
        for file in glob.glob(rf"{test_run_foldername}{os.sep}*screenshot*"):
            _copy_to_destination_folder(test_run_foldername, file.split(os.sep)[-1], results_foldername)

        for file in glob.glob(rf"{test_run_foldername}{os.sep}*capture*"):
            _copy_to_destination_folder(test_run_foldername, file.split(os.sep)[-1], results_foldername)


def log_report_results(number_of_test_runs: int, failing_suites: []):
    logger.info(f"Log available at: file://{os.getcwd()}{os.sep}{results_foldername}{os.sep}log.html")
    logger.info(f"Report available at: file://{os.getcwd()}{os.sep}{results_foldername}{os.sep}report.html")
    logger.info(f"Number of test runs: {number_of_test_runs}")

    if failing_suites:
        logger.info(f"Number of failing suites: {len(failing_suites)}")
        logger.info(f"Failing suites:")
        [logger.info(r"  * file://" + suite) for suite in failing_suites]
    else:
        logger.info("\nAll tests passed!")


def _merge_test_reports(test_results_folder):
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
        f"{test_results_folder}/output.xml",
    ]
    robot_rebot_cli(merge_args, exit=False)


def _copy_to_destination_folder(source_folder: str, source_file: str, destination_folder: str):
    source_file_path=f"{source_folder}{os.sep}{source_file}"
    destination_file_path=f"{destination_folder}{os.sep}{source_file}"
    if os.path.isfile(source_file_path):
        shutil.copy(source_file_path, destination_file_path)
    else:
        shutil.copytree(source_file_path, destination_file_path, dirs_exist_ok=True)