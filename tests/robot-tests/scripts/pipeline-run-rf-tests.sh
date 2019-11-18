#!/bin/bash

# NOTE(mark): The admin password to access to Admin app is stored in the CI pipeline
# as a secret variable, which means it cannot be accessed as a normal environment
# variable, and instead must be passed as an argument to this script

[ "$#" -eq 2 ] || { echo "Requires two arguments. Usage: 'pipeline-run-rf-tests.sh [admin_pass] [env]'. Exiting..."; exit 1; }

google-chrome-stable --version

python -m pip install --upgrade pip
pip install pipenv
pipenv install
pipenv run python run_tests.py -e $2 --ci --chromedriver 78.0.3904.70 --admin-pass $1
