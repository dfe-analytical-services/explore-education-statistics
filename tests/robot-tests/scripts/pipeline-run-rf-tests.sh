#!/bin/bash

cd tests/robot-tests
pipenv install
pipenv run python run_tests.py --ci
