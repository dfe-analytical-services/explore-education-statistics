import argparse
import subprocess

# NOTE(mark): The slack webhook url, and admin and analyst passwords to access to Admin app are
# stored in the CI pipeline as secret variables, which means they cannot be accessed as normal
# environment variables, and instead must be passed as an argument to this script.

# WARNING: If any of the passwords contain a "$", bash/Azure devops will not pass the variable to run_tests.py correctly.
# Our current solution to this is to escape any dollars where the password is stored
# i.e. password "hello$world" should be stored as "hello\$world"


def run_tests_pipeline():
    assert args.admin_pass, "Provide an admin password with an '--admin-pass PASS' argument"
    assert args.analyst_pass, "Provide an analyst password with an '--analyst-pass PASS' argument"
    assert args.expiredinvite_pass, "Provide an expiredinvite password with an '--expiredinvite-pass PASS' argument"
    assert args.slack_webhook_url, "Please provide slack webhook URL"
    assert args.env, "Provide an environment with an '--env ENV' argument"
    assert args.file, "Provide a file/dir to run with an '--file FILE/DIR' argument"
    assert args.processes, "Provide a number of processes to run with the '--processes NUM' argument"
    valid_environments = ["dev", "test", "preprod", "prod", "ci"]

    if args.env not in valid_environments:
        raise Exception(f"Invalid environment provided: {args.env}. Valid environments: {valid_environments}")

    subprocess.check_call(["google-chrome-stable", "--version"])
    subprocess.check_call("python -m pip install --upgrade pip", shell=True)
    subprocess.check_call("pip install pipenv", shell=True)
    subprocess.check_call("pipenv install", shell=True)

    def get_test_command() -> str:
        if args.file == "tests/general_public/check_snapshots.robot":
            return f"pipenv run python run_tests.py --admin-pass {args.admin_pass} --analyst-pass {args.analyst_pass} --expiredinvite-pass {args.expiredinvite_pass} --slack-webhook-url {args.slack_webhook_url} --env {args.env} --file {args.file} --ci --processes {args.processes}"
        else:
            return f"pipenv run python run_tests.py --admin-pass {args.admin_pass} --analyst-pass {args.analyst_pass} --expiredinvite-pass {args.expiredinvite_pass} --slack-webhook-url {args.slack_webhook_url} --env {args.env} --file {args.file} --ci --processes {args.processes} --enable-slack"

    subprocess.run(get_test_command(), shell=True)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        prog="pipenv run python run_tests_pipeline.py",
        description="Use this script in a CI environment to run UI tests",
    )

    parser.add_argument("--admin-pass", dest="admin_pass", help="BAU admin password", required=True)

    parser.add_argument("--analyst-pass", dest="analyst_pass", help="Analyst password", required=True)

    parser.add_argument(
        "--expiredinvite-pass", dest="expiredinvite_pass", help="ExpiredInvite account password", required=True
    )

    parser.add_argument(
        "--slack-webhook-url",
        dest="slack_webhook_url",
        help="slack webhook URL for sending test reports",
        required=True,
    )

    parser.add_argument(
        "--env", dest="env", help=f"environment to run tests against (dev, test, pre-prod, prod & ci)", required=True
    )

    parser.add_argument("--file", dest="file", help="the directory or file to test", required=True)

    parser.add_argument("--processes", dest="processes", help="number of processes to run", required=True)

    args = parser.parse_args()
    run_tests_pipeline()
