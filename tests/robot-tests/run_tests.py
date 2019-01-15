#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Options:
-v|--visual : Don't run the tests headless. Usage: "./run_tests.py -v"

-e|--env : Run against specific environment, "local", "test", "stage", "prod".
Usage: "./run_tests.py -e test"

-h|--happypath: Run happypath tests only. Usage: "./run_tests.py -h"

-b BROWSER|--browser BROWSER : Run a different browser to the default, chrome.
Usage: "./run_tests.py -b firefox"

-f FILE|--file FILE : To run a specific test file or folder instead of the
entire tests/ directory. Usage: "./run_tests.py -f tests/directory/" OR "./run_tests.py -f tests/directory/suite.robot"

-i INTERPRETER|--interp INTERPRETER : Run tests through a different interpreter
than cpython. Mainly for using pabot, which runs test suites in parallel.
Usage: "./run_tests.py -i pabot"

-p|--profile : Additionally output python profile information
AND keyword profile information. Outputs log files to test-results directory.
Usage: "./run_tests.py -p"

"""

import sys

arguments = []
headless = True
environment = "test"
happypath = False
profile = False
tests = "tests/"
browser = "chrome"
interp = "robot"
url = ""

for i in range(1, len(sys.argv)):
    if sys.argv[i] == "-v" or sys.argv[i] == "--visual":
        headless = False
    elif sys.argv[i] == "-e" or sys.argv[i] == "--env":
        environment = sys.argv[i+1]  # NOTE: could add error checking...
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

arguments += ["--outputdir", "test-results/", "--exclude", "Failing",
              "--exclude", "UnderConstruction"]

if happypath:
    arguments += ["--include", "HappyPath"]

if headless:
    arguments += ["-v", "headless:1"]
else:
    arguments += ["-v", "headless:0"]

arguments += ["-v", "env:" + environment]

arguments += ["-v", "browser:" + browser]

arguments += [tests]

if interp == "robot":
    from robot import run_cli
    if profile:
        # Python profiling
        import cProfile
        cProfile.run('run_cli(arguments)', 'profile-data')
        import pstats
        stream = open('test-results/python-profiling-results.log', 'w')
        p = pstats.Stats('profile-data', stream=stream)
        p.sort_stats('time')
        # p.sort_stats('cumulative')
        p.print_stats()
        import os
        os.remove('profile-data')

        # Keyword profiling
        import scripts.keyword_profile as kp
        kp.run_keyword_profile('test-results/output.xml',
                               printresults=False,
                               writepath='test-results/keyword-profiling-results.log')
        print("\nProfiling logs created in test-results/")
    else:
        run_cli(arguments)
elif interp == "pabot":
    from pabot.pabot import main
    if profile:
        # Python profiling
        import cProfile
        cProfile.run('main(arguments)', 'profile-data')

        import pstats
        stream = open('test-results/python-profiling-results.log', 'w')
        p = pstats.Stats('profile-data', stream=stream)
        p.sort_stats('time')
        # p.sort_stats('cumulative')
        p.print_stats()
        import os
        os.remove('profile-data')

        # Keyword profiling
        import scripts.keyword_profile as kp
        kp.run_keyword_profile('test-results/output.xml',
                               printresults=False,
                               writepath='test-results/keyword-profiling-results.log')
        print("\nProfiling logs created in test-results/")
    else:
        main(arguments)
