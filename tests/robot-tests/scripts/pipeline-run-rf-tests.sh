#!/bin/bash

robot_env=$1
python -m pip install --upgrade pip
pip install pipenv
pipenv install
pipenv run python run_tests.py --ci -e ${robot_env}
