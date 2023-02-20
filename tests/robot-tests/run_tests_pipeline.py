import argparse
import subprocess
from tests.libs.slack import SlackService

# NOTE(mark): The admin and analyst passwords to access the Admin app are
# stored in the CI pipeline as secret variables, which means they cannot be accessed as normal
# environment variables, and instead must be passed as an argument to this script.

# WARNING: If any of the passwords contain a "$", bash/Azure devops will not pass the variable to run_tests.py correctly.
# Our current solution to this is to escape any dollars where the password is stored
# i.e. password "hello$world" should be stored as "hello\$world"

def run_tests_pipeline() -> bool:
    assert args.admin_pass, "Provide an admin password with an '--admin-pass PASS' argument"
    assert args.analyst_pass, "Provide an analyst password with an '--analyst-pass PASS' argument"
    assert args.expiredinvite_pass, "Provide an expiredinvite password with an '--expiredinvite-pass PASS' argument"
    assert args.env, "Provide an environment with an '--env ENV' argument"
    assert args.file, "Provide a file/dir to run with an '--file FILE/DIR' argument"
    assert args.processes, "Provide a number of processes to run with the '--processes NUM' argument"

    # subprocess.check_call(["google-chrome-stable", "--version"])
    # subprocess.check_call("python -m pip install --upgrade pip", shell=True)
    # subprocess.check_call("pip install pipenv", shell=True)
    # subprocess.check_call("pipenv install", shell=True)

    run_tests_command = f"pipenv run python3 run_tests.py --admin-pass {args.admin_pass} --analyst-pass {args.analyst_pass} --expiredinvite-pass {args.expiredinvite_pass} --env {args.env} --file {args.file} --ci --processes {args.processes}"

    result = subprocess.run(run_tests_command, shell=True)

    if (result.returncode == 0):
        return True

    if args.reruns is not None and args.reruns > 0:
        
        run_tests_command += " --rerun-failed-suites"
    
        for rerun in range(args.reruns):
            result = subprocess.run(run_tests_command, shell=True)
    
            if (result.returncode == 0):
                return True
    
    return False


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
        "--env", dest="env", help=f"environment to run tests against (local, dev, test, pre-prod, prod & ci)", required=True,
        choices=["local", "dev", "test", "preprod", "prod", "ci"]
    )

    parser.add_argument("--file", dest="file", help="the directory or file to test", required=True)

    parser.add_argument("--processes", dest="processes", help="number of processes to run", type=int, required=True)

    parser.add_argument("--reruns", dest="reruns", help="number of reruns to attempt upon failure", type=int, required=False)

    args = parser.parse_args()
    result=run_tests_pipeline()

    slack_service = SlackService()
    slack_service.send_test_report(args.env, args.file)
    
    if not result:
        raise Exception('Failure running tests')