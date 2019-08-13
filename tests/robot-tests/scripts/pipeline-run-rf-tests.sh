#!/bin/bash

[ "$#" -eq 1 ] || { echo "1 arguments required, $# given"; exit 1; }

google-chrome-stable --version

python -m pip install --upgrade pip
pip install pipenv
pipenv install
pipenv run python run_tests.py -e ci --chromedriver 76.0.3809.68 --admin-pass $1
