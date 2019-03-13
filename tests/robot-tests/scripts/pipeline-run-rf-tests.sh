#!/bin/bash

pipenv install
pipenv run python run_tests.py --ci
