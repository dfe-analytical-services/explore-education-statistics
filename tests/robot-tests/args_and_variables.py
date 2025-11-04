import argparse
import os

from dotenv import load_dotenv


# Create a parser for our CLI arguments.
def create_argument_parser() -> argparse.ArgumentParser:
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
        "--processes",
        dest="processes",
        type=int,
        default=4,
        help="how many processes should be used when using the pabot interpreter",
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
        "--excludetags",
        dest="exclude_tags",
        nargs="?",
        metavar="{tag(s)}",
        help="specify tests you don't wish to run by tag",
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
        "--custom-env",
        dest="custom_env",
        default=None,
        help="load a custom .env file (must be in ~/robot-tests directory)",
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
    parser.add_argument(
        "--analyst-pass", dest="analyst_pass", default=None, help="manually specify the analyst password"
    )
    parser.add_argument(
        "--expiredinvite-pass",
        dest="expiredinvite_pass",
        default=None,
        help="manually specify the expiredinvite user password",
    )
    parser.add_argument(
        "--noinvite-pass",
        dest="noinvite_pass",
        default=None,
        help="manually specify the noinvite user password",
    )
    parser.add_argument(
        "--pendinginvite-pass",
        dest="pendinginvite_pass",
        default=None,
        help="manually specify the pendinginvite user password",
    )
    parser.add_argument(
        "--reseed",
        dest="reseed",
        action="store_true",
        help="run the seed data generation scripts against the target environment",
    )

    return parser


def initialise() -> argparse.Namespace:
    args = create_argument_parser().parse_args()

    if args.reseed and args.env == "prod":
        raise Exception(f"Cannot generate seed data against environment {args.env}")

    if includes_data_changing_tests(args) and args.env not in ["local", "dev"]:
        raise Exception(f"Cannot run tests that change data on environment {args.env}")

    load_environment_variables(args)
    store_credential_environment_variables(args)
    validate_environment_variables()
    return args


def load_environment_variables(arguments: argparse.Namespace):
    if arguments.custom_env:
        load_dotenv(arguments.custom_env)
    else:
        load_dotenv(".env." + arguments.env)


def validate_environment_variables():
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
        "WAIT_DATA_FILE_IMPORT",
        "IDENTITY_PROVIDER",
        "WAIT_CACHE_EXPIRY",
        "ADMIN_EMAIL",
        "ADMIN_PASSWORD",
        "ANALYST_EMAIL",
        "ANALYST_PASSWORD",
        "EXPIRED_INVITE_USER_EMAIL",
        "EXPIRED_INVITE_USER_PASSWORD",
        "NO_INVITE_USER_EMAIL",
        "NO_INVITE_USER_PASSWORD",
        "PENDING_INVITE_USER_EMAIL",
        "PENDING_INVITE_USER_PASSWORD",
        "PUBLISHER_FUNCTIONS_URL",
    ]

    for env_var in required_env_vars:
        assert os.getenv(env_var) is not None, f"Environment variable {env_var} is not set"


# If running all tests, or admin, admin_and_public, admin_and_public_2, public_api or seed_data suites,
# these change data on environments and require test themes and user authentication.
#
# We check for both explicit forward slashes AND OS-specific separators here, as in Windows
# we can expect to get either scenario depending upon how we're running these tests e.g.
# Git Bash versus Command Prompt versus IDEs.  {os.sep} in Windows always evaluates to
# `\\` whereas any of these run options above may choose to either supply forward slashes or
# back slashes.
def includes_data_changing_tests(arguments: argparse.Namespace):
    return (
        arguments.tests == "tests"
        or arguments.tests == "tests/"
        or arguments.tests == f"tests{os.sep}"
        or f"{os.sep}admin" in arguments.tests
        or "/admin" in arguments.tests
        or f"{os.sep}public_api" in arguments.tests
        or "/public_api" in arguments.tests
        or f"{os.sep}seed_data" in arguments.tests
        or "/seed_data" in arguments.tests
    )


def store_credential_environment_variables(arguments: argparse.Namespace):
    if arguments.admin_pass:
        os.environ["ADMIN_PASSWORD"] = arguments.admin_pass
    if arguments.analyst_pass:
        os.environ["ANALYST_PASSWORD"] = arguments.analyst_pass
    if arguments.expiredinvite_pass:
        os.environ["EXPIRED_INVITE_USER_PASSWORD"] = arguments.expiredinvite_pass
    if arguments.noinvite_pass:
        os.environ["NO_INVITE_USER_PASSWORD"] = arguments.noinvite_pass
    if arguments.pendinginvite_pass:
        os.environ["PENDING_INVITE_USER_PASSWORD"] = arguments.pendinginvite_pass
