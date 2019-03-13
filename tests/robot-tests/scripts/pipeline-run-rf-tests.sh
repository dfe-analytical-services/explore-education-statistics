#!/bin/bash

cd robot-tests
pipenv install
pipenv run python run_tests.py --ci
