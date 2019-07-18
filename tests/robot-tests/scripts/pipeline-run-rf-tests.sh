#!/bin/bash

python -m pip install --upgrade pip
pip install pipenv
pipenv install
pipenv run python run_tests.py -e ci --chromedriver 74.0.3729.6
