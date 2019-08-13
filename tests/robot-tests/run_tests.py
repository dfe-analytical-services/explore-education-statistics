#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Run 'python run_tests.py -h' to see argument options
"""

import sys
import os
import argparse
from robot import run_cli as robot_run_cli
from pabot.pabot import main as pabot_run_cli
import cProfile
import pstats
import scripts.keyword_profile as kp
import chromedriver_install as cdi
from dotenv import load_dotenv

# Parse arguments
parser = argparse.ArgumentParser(prog="pipenv run python run_tests.py",
                                 description="Use this script to run the UI tests, locally or as part of the CI pipeline, against the environment of your choosing")
parser.add_argument("-b", "--browser",
                    dest="browser",
                    default="chrome",
                    choices=["chrome", "firefox"],
                    help="name of the browser you wish to run the tests with (NOTE: Only chromedriver is automatically installed!)")
parser.add_argument("-i", "--interp",
                    dest="interp",
                    default="pabot",
                    choices=["pabot", "robot"],
                    help="interpreter to use to run the tests")
parser.add_argument("-e", "--env",
                    dest="env",
                    default="dev",
                    choices=["local", "dev", "dev02", "dev03", "test", "prod", "ci"],
                    help="the environment to run the tests against")
parser.add_argument("-f", "--file",
                    dest="tests",
                    metavar="{file/dir}",
                    default="tests/",
                    help="test suite or folder of tests suites you wish to run")
parser.add_argument("-t", "--tags",
                    dest="tags",
                    nargs="?",
                    metavar="{tag(s)}",
                    help="specify tests you wish to run by tag")
parser.add_argument("-v", "--visual",
                    dest="visual",
                    action="store_true",
                    help="display browser window that the tests run in")
parser.add_argument("-p", "--profile",
                    dest="profile",
                    action="store_true",
                    help="output profiling information")
parser.add_argument("--happypath",
                    dest="happypath",
                    action="store_true",
                    help="only run happy-path tests")
parser.add_argument("--chromedriver",
                    dest="chromedriver_version",
                    default="74.0.3729.6",
                    metavar="{version}",
                    help="specify which version of chromedriver to use")
"""
NOTE(mark): The admin password to access the admin app is stored in the CI pipeline 
            as a secret variable, which means it cannot be accessed like a normal 
            environment variable, and instead must be passed to this script as 
            an argument.
"""
parser.add_argument("--admin-pass",
                    dest="admin_pass",
                    default=None,
                    help="manually specify the admin password")
args = parser.parse_args()

if args.admin_pass:
    os.environ['ADMIN_PASSWORD'] = args.admin_pass

# Default values
timeout = 20
implicit_wait = 20

# Set robotArgs
robotArgs = ["--outputdir", "test-results/", "--exclude", "Failing",
             "--exclude", "UnderConstruction"]

if args.tags:
    robotArgs += ["--include", args.tags]

if args.env == "ci":
    robotArgs += ["--xunit", "xunit", "-v", "timeout:" + str(timeout), "-v", "implicit_wait:" + str(implicit_wait)]
    # NOTE(mark): Ensure secrets aren't visible in CI logs/reports
    robotArgs += ["--removekeywords", "name:library.user logs into microsoft online"]
    robotArgs += ["--removekeywords", "name:operatingsystem.environment variable should be set"]

else:
    if args.env == 'local':
        robotArgs += ['--exclude', 'NotAgainstLocal']
    load_dotenv(os.path.join(os.path.dirname(__file__), '.env.' + args.env))

if os.getenv('PUBLIC_URL') is None or os.getenv('ADMIN_URL') is None:
    print("PUBLIC_URL and/or ADMIN_URL are None -- .env.{env} file needs to be set")
    sys.exit(1)

if args.visual:
    robotArgs += ["-v", "headless:0"]
else:
    robotArgs += ["-v", "headless:1"]

robotArgs += ["-v", "browser:" + args.browser]
robotArgs += [args.tests]

# Install chromedriver and add it to PATH
cdi.install(file_directory='./webdriver/',
                   verbose=False,
                   chmod=True,
                   overwrite=False,
                   version=args.chromedriver_version)
os.environ["PATH"] += os.pathsep + os.getcwd() + os.sep + 'webdriver'

# Run tests
if args.interp == "robot":
    if args.profile:
        # Python profiling
        cProfile.run('robot_run_cli(robotArgs)', 'profile-data')
        stream = open('test-results/python-profiling-results.log', 'w')
        p = pstats.Stats('profile-data', stream=stream)
        p.sort_stats('time')
        # p.sort_stats('cumulative')
        p.print_stats()
        os.remove('profile-data')

        # Keyword profiling
        kp.run_keyword_profile('test-results/output.xml',
                               printresults=False,
                               writepath='test-results/keyword-profiling-results.log')
        print("\nProfiling logs created in test-results/")
    else:
        robot_run_cli(robotArgs)
elif args.interp == "pabot":
    if args.profile:
        # Python profiling
        cProfile.run('pabot_run_cli(robotArgs)', 'profile-data')
        stream = open('test-results/python-profiling-results.log', 'w')
        p = pstats.Stats('profile-data', stream=stream)
        p.sort_stats('time')
        # p.sort_stats('cumulative')
        p.print_stats()
        os.remove('profile-data')

        # Keyword profiling
        kp.run_keyword_profile('test-results/output.xml',
                               printresults=False,
                               writepath='test-results/keyword-profiling-results.log')
        print("\nProfiling logs created in test-results/")
    else:
        pabot_run_cli(robotArgs)
