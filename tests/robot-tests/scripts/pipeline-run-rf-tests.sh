#!/bin/bash

cd tests/robot-tests
pipenv install
#pipenv run robot --outputdir test-results/ --xunit xunit -v headless:1 -v env:test -v browser:chrome -v timeout:10 -v implicit_wait:10 tests/
#pipenv run pabot --outputdir test-results/ --xunit xunit -v headless:1 -v env:test -v browser:chrome -v timeout:10 -v implicit_wait:10 tests/
pipenv run python run_tests.py --ci -i pabot
