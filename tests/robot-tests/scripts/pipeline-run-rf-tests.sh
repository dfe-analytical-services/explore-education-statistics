#!/bin/bash

google-chrome-stable --version

python -m pip install --upgrade pip
pip install pipenv
pipenv install
pipenv run python run_tests.py -e ci --chromedriver 76.0.3809.68
