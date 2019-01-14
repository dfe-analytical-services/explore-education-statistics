#!/usr/bin/env python

"""
*** Robot Framework Test Runner Script ***

Options:
-v|--visual : Don't run the tests headless. Usage: "./run_tests.py -v"

-b BROWSER|--browser BROWSER : Run a different browser to the default, chrome.
Usage: "./run_tests.py -b firefox"

-f FILE|--file FILE : To run a specific test file or folder instead of the
entire tests/ directory. Usage: "./run_tests.py -f tests/main.robot"

-i INTERPRETER|--interp INTERPRETER : Run tests through a different interpreter
than cpython. Usage: "./run_tests.py -i pabot"

-p|--profile : Output robot framework results AND python profile information
AND keyword profile information. Outputs log files to test-results directory.
Usage: "./run_tests.py -p"

-c|--complete: Run all available tests. Without this, tests expected to provide
little value running everytime are ignored. Usage: "./run_tests.py -c"
"""

import sys

arguments = []
headless = True
happypath = True
profile = False
tests = "tests/"
browser = "chrome"
interp = "robot"
url = ""

for i in range(1, len(sys.argv)):
    # print(i, sys.argv[i])
    if sys.argv[i] == "-v" or sys.argv[i] == "--visual":
        headless = False
    elif sys.argv[i] == "-c" or sys.argv[i] == "--complete":
        happypath = False
    elif sys.argv[i] == "-p" or sys.argv[i] == "--profile":
        profile = True
    elif sys.argv[i] == "-b" or sys.argv[i] == "--browser":
        browser = sys.argv[i+1]  # NOTE: could add error checking...
    elif sys.argv[i] == "-i" or sys.argv[i] == "--interp":
        interp = sys.argv[i+1]  # NOTE: could add error checking...
    elif sys.argv[i] == "-f" or sys.argv[i] == "--file":
        tests = sys.argv[i+1]  # NOTE: could add error checking...
    elif sys.argv[i] == "-u" or sys.argv[i] == "--url":
        url = sys.argv[i+1]  # NOTE: could add error checking...

arguments += ["--outputdir", "test-results/", "--exclude", "Failing",
              "--exclude", "UnderConstruction"]

if happypath:
    arguments += ["--include", "HappyPath"]

if headless:
    arguments += ["-v", "headless:1"]
else:
    arguments += ["-v", "headless:0"]

arguments += ["-v", "browser:" + browser]

if url:
    arguments += ["-v", "url:" + url]

arguments += [tests]

# print(arguments)

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
