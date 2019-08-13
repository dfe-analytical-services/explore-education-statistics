#!/bin/bash

# NOTE(mark): The admin password to access to Admin app is stored in the CI pipeline
# as a secret variable, which means it cannot be accessed as a normal environment
# variable, and instead must be passed as an argument to this script
[ "$#" -eq 1 ] || { echo "Please provide a single argument, the admin password. Exiting..."; exit 1; }

google-chrome-stable --version

python -m pip install --upgrade pip
pip install pipenv
pipenv install
pipenv run python run_tests.py -e ci --chromedriver 76.0.3809.68 --admin-pass $1
