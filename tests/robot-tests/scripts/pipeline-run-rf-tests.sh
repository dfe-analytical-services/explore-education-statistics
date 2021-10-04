#!/bin/bash

# NOTE(mark): The admin and analyst passwords to access to Admin app are stored in the CI pipeline
# as secret variables, which means they cannot be accessed as normal environment
# variables, and instead must be passed as an argument to this script

[ "$#" -eq 4 ] || { echo "Requires four arguments. Usage: 'pipeline-run-rf-tests.sh [admin_pass] [analyst_pass] [env] [test_file]'. Exiting..."; exit 1; }

google-chrome-stable --version

python -m pip install --upgrade pip
pip install pipenv
pipenv install
pipenv run python run_tests.py --admin-pass $1 --analyst-pass $2 -e $3 --ci --file $4 --processes 4

if [ $? -ne 0 ]
then
  echo "Rerunning failed test suites..."
  pipenv run python run_tests.py --admin-pass $1 --analyst-pass $2 -e $3 --ci --file $4 --processes 4 --rerun-failed-suites
fi