import argparse
import os

import pabot.pabot as pabot
import reports
import robot
from tests.libs.logger import get_logger

logger = get_logger(__name__)


def create_robot_arguments(arguments: argparse.Namespace, test_run_folder: str) -> list[str]:
    robot_args = [
        "--name",
        "UI Tests",
        "--outputdir",
        f"{test_run_folder}/",
        "--xunit",
        "xunit",
    ]

    robot_args += _create_include_and_exclude_args(arguments)

    robot_args += ["-v", f"timeout:{os.getenv('TIMEOUT')}", "-v", f"implicit_wait:{os.getenv('IMPLICIT_WAIT')}"]

    if arguments.fail_fast:
        robot_args += ["--exitonfailure"]
    if arguments.print_keywords:
        robot_args += ["--listener", "listeners/KeywordListener.py"]
    if arguments.ci:
        # NOTE(mark): Ensure secrets aren't visible in CI logs/reports
        robot_args += ["--removekeywords", "name:operatingsystem.environment variable should be set"]
        robot_args += ["--removekeywords", "name:common.user goes to url"]  # To hide basic auth credentials

    if arguments.visual:
        robot_args += ["-v", "headless:0"]
    else:
        robot_args += ["-v", "headless:1"]
    if arguments.prompt_to_continue:
        robot_args += ["-v", "prompt_to_continue_on_failure:1"]
    if arguments.debug:
        robot_args += ["--loglevel", "DEBUG"]
    robot_args += ["-v", "browser:" + arguments.browser]
    # We want to add arguments on the first rerun attempt, but on subsequent attempts, we just want
    # to change rerunfailedsuites xml file we use
    robot_args += ["--output", "output.xml"]
    return robot_args


def _create_include_and_exclude_args(arguments: argparse.Namespace) -> list[str]:
    include_exclude_args = ["--exclude", "Failing"]
    include_exclude_args += [
        "--exclude",
        "ReleaseRedesign",
    ]  # TODO: EES-6843: Remove this line when release redesign's feature flag has been removed
    include_exclude_args += ["--exclude", "UnderConstruction"]
    include_exclude_args += ["--exclude", "VisualTesting"]
    if arguments.exclude_tags:
        include_exclude_args += ["--exclude", arguments.exclude_tags]
    if arguments.tags:
        include_exclude_args += ["--include", arguments.tags]
    if arguments.reseed:
        include_exclude_args += ["--include", "SeedDataGeneration"]
    else:
        include_exclude_args += ["--exclude", "SeedDataGeneration"]
    if arguments.env == "local":
        include_exclude_args += ["--include", "Local", "--exclude", "NotAgainstLocal"]
    if arguments.env == "dev":
        include_exclude_args += ["--include", "Dev", "--exclude", "NotAgainstDev"]
    if arguments.env == "test":
        include_exclude_args += ["--include", "Test", "--exclude", "NotAgainstTest", "--exclude", "AltersData"]
    # fmt off
    if arguments.env == "preprod":
        include_exclude_args += ["--include", "Preprod", "--exclude", "AltersData", "--exclude", "NotAgainstPreProd"]
    # fmt on
    if arguments.env == "prod":
        include_exclude_args += ["--include", "Prod", "--exclude", "AltersData", "--exclude", "NotAgainstProd"]
    return include_exclude_args


def execute_tests(arguments: argparse.Namespace, test_run_folder: str, path_to_previous_report_file: str):
    robot_args = create_robot_arguments(arguments, test_run_folder)

    if arguments.interp == "robot":
        if path_to_previous_report_file is not None:
            robot_args += ["--rerunfailedsuites", path_to_previous_report_file]

        robot_args += [arguments.tests]

        logger.info(f"Performing test run with Robot")
        robot.run_cli(robot_args, exit=False)

    elif arguments.interp == "pabot":
        robot_args = ["--processes", arguments.processes] + robot_args

        if path_to_previous_report_file is not None:
            path_to_filtered_report_file = "_filtered.xml".join(path_to_previous_report_file.rsplit(".xml", 1))
            reports.filter_out_passing_suites_from_report_file(
                path_to_previous_report_file, path_to_filtered_report_file
            )

            logger.info(
                f"Generated filtered report file containing only failing suites at {path_to_filtered_report_file}"
            )
            robot_args = ["--suitesfrom", path_to_filtered_report_file] + robot_args

        robot_args += [arguments.tests]

        logger.info(f"Performing test run with Pabot ({arguments.processes} processes)")
        pabot.main_program(robot_args)
