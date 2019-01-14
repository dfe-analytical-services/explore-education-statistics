#!/bin/bash

cd tests/robot-tests
pipenv install
pipenv run robot --outputdir test-results/ --xunit xunit -v headless:1 -v env:test -v browser:chrome -v timeout:5 -v implicit_wait:5 tests/
