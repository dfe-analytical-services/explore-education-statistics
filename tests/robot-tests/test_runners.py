import os
import reports
import argparse
import pabot.pabot as pabot
import robot
from tests.libs.logger import get_logger

logger = get_logger(__name__)


def create_robot_arguments(arguments: argparse.Namespace, test_run_folder: str) -> []:
    robot_args = [
        "--name",
        "UI Tests",
        "--outputdir",
        f"{test_run_folder}/",
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
    
    process_includes_and_excludes(robot_args, arguments)
    
    if arguments.visual:
        robot_args += ["-v", "headless:0"]
    else:
        robot_args += ["-v", "headless:1"]
    if os.getenv("RELEASE_COMPLETE_WAIT"):
        robot_args += ["-v", f"release_complete_wait:{os.getenv('RELEASE_COMPLETE_WAIT')}"]
    if arguments.prompt_to_continue:
        robot_args += ["-v", "prompt_to_continue_on_failure:1"]
    if arguments.debug:
        robot_args += ["--loglevel", "DEBUG"]
    robot_args += ["-v", "browser:" + arguments.browser]
    # We want to add arguments on the first rerun attempt, but on subsequent attempts, we just want
    # to change rerunfailedsuites xml file we use
    robot_args += ["--output", "output.xml"]

    return robot_args


def process_includes_and_excludes(robot_args: [], arguments: argparse.Namespace):
    if arguments.reseed:
        robot_args += ["--include", "SeedDataGeneration"]
    else:
        robot_args += ["--exclude", "SeedDataGeneration"]
    if arguments.env == "local":
        robot_args += ["--include", "Local", "--exclude", "NotAgainstLocal"]
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


def execute_tests(arguments: argparse.Namespace, test_run_folder: str, path_to_previous_report_file: str):
    
    robot_args = create_robot_arguments(arguments, test_run_folder)
    
    if arguments.interp == "robot":

        if path_to_previous_report_file is not None:
            robot_args += ["--rerunfailedsuites", path_to_previous_report_file]
        
        robot_args += [arguments.tests]

        robot.run_cli(robot_args, exit=False)

    elif arguments.interp == "pabot":

        logger.info('Performing test run with Pabot')
        
        if path_to_previous_report_file is not None:
            
            logger.info(f'Re-running failed suites from {path_to_previous_report_file}')
        
            path_to_filtered_report_file = '_filtered.xml'.join(path_to_previous_report_file.rsplit('.xml', 1))

            reports.filter_out_passing_suites_from_report_file(path_to_previous_report_file, path_to_filtered_report_file)
            
            logger.info(f'Generated filtered report file containing only failing suites at {path_to_filtered_report_file}')

            robot_args = ['--suitesfrom', path_to_filtered_report_file] + robot_args

        robot_args += [arguments.tests]

        pabot.main_program(robot_args)
