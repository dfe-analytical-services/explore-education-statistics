#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Options:
-v|--visual : Don't run the tests headless. Usage: "pipenv run python run_tests.py -v"

-e|--env : Run against specific environment, "local", "test", "stage", "prod".
Usage: "pipenv run python run_tests.py -e test"

-h|--happypath: Run happypath tests only. Usage: "pipenv run python run_tests.py -h"

-b BROWSER|--browser BROWSER : Run a different browser to the default, chrome.
Usage: "pipenv run python run_tests.py -b firefox". You will need to install the webdriver for that browser (e.g. geckodriver for firefox)

-f FILE|--file FILE : To run a specific test file or folder instead of the
entire tests/ directory. Usage: "pipenv run python run_tests.py -f tests/directorynamehere/" OR "pipenv run python run_tests.py -f tests/directorynamehere/testsuitenamehere.robot"

-i INTERPRETER|--interp INTERPRETER : Run tests through a different interpreter
than pabot. Mainly for using robot, which doesn't run the test suites in parallel.
Usage: "pipenv run python run_tests.py -i robot"

-p|--profile : Additionally output python profile information
AND keyword profile information. Outputs log files to test-results directory.
Usage: "pipenv run python run_tests.py -p"

--ci : Add arguments for running the tests as part of the CI pipeline
"""

import os
import sys
from robot import run_cli as robot_run_cli
from pabot.pabot import main as pabot_run_cli
import cProfile
import pstats
import scripts.keyword_profile as kp
import chromedriver_install as cdi
from dotenv import load_dotenv
# import scripts.warm_up_servers as warm_up_servers

arguments = []
headless = True
happypath = False
profile = False
ci = False
tests = "tests/"
browser = "chrome"
interp = "pabot"

# Get basicAuth credentials
basicAuthUser = ""
basicAuthPass = ""
load_dotenv()
if os.getenv('publicAppBasicAuthUsername') and os.getenv('publicAppBasicAuthPassword'):
    basicAuthUser = os.getenv('publicAppBasicAuthUsername')
    basicAuthPass = os.getenv('publicAppBasicAuthPassword')
else:
    print("No publicAppBasicAuth credentials found! If running locally, have you created a .env file?")
    sys.exit()

env = "test"  # by default, run tests against test environment
url = "about:blank"
localUrl = "http://localhost:3000"
localAdminUrl = ""
testUrl = "https://%s:%s@public-explore-education-statistics-test.azurewebsites.net" % (basicAuthUser, basicAuthPass)
testAdminUrl = "https://admin-explore-education-statistics-test.azurewebsites.net"
stageUrl = "https://public-explore-education-statistics-stage.azurewebsites.net"
stageAdminUrl = "https://admin-explore-education-statistics-stage.azurewebsites.net"

timeout = 10
implicit_wait = 10

# Process arguments
for i in range(1, len(sys.argv)):
    if sys.argv[i] == "-v" or sys.argv[i] == "--visual":
        headless = False
    elif sys.argv[i] == "-e" or sys.argv[i] == "--env":
        env = sys.argv[i+1]  # NOTE: could add error checking...
    elif sys.argv[i] == "-h" or sys.argv[i] == "--happypath":
        happypath = True
    elif sys.argv[i] == "-b" or sys.argv[i] == "--browser":
        browser = sys.argv[i+1]  # NOTE: could add error checking...
    elif sys.argv[i] == "-f" or sys.argv[i] == "--file":
        tests = sys.argv[i+1]  # NOTE: could add error checking...
    elif sys.argv[i] == "-i" or sys.argv[i] == "--interp":
        interp = sys.argv[i+1]  # NOTE: could add error checking...
    elif sys.argv[i] == "-p" or sys.argv[i] == "--profile":
        profile = True
    elif sys.argv[i] == "--ci":
      ci = True

arguments += ["--outputdir", "test-results/", "--exclude", "Failing",
              "--exclude", "UnderConstruction"]

if happypath:
    arguments += ["--include", "HappyPath"]

if ci:
  arguments += ["--xunit", "xunit", "-v", "timeout:" + str(timeout), "-v", "implicit_wait:" + str(implicit_wait)]

if headless:
    arguments += ["-v", "headless:1"]
else:
    arguments += ["-v", "headless:0"]

if env == 'local':
    url = localUrl
    urlAdmin = localAdminUrl
elif env == 'test':
    url = testUrl
    urlAdmin = testAdminUrl
elif env == 'stage' or env == "staging":
    url = stageUrl
    urlAdmin = stageAdminUrl
elif env == "prod" or env == "live":
    url = prodUrl
    urlAdmin = prodAdminUrl
else:
    raise Exception('Invalid environment \"' + env + '\"! Must be \"local\", \"test\", \"stage\", or \"prod\"!')

arguments += ["-v", "url:" + url]
arguments += ["-v", "urlAdmin:" + urlAdmin]
arguments += ["-v", "browser:" + browser]

arguments += [tests]

# Install Chromedriver and add it to PATH
path = cdi.install(file_directory='./webdriver/', verbose=False, chmod=True, overwrite=False, version=None)
os.environ["PATH"] += os.pathsep + os.getcwd() + os.sep + 'webdriver'

# Wait until environment is warmed up
# warm_up_servers.wait_for_server(url)

# Run tests
if interp == "robot":
    if profile:
        # Python profiling
        cProfile.run('robot_run_cli(arguments)', 'profile-data')
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
        robot_run_cli(arguments)
elif interp == "pabot":
    if profile:
        # Python profiling
        cProfile.run('pabot_run_cli(arguments)', 'profile-data')
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
        pabot_run_cli(arguments)
