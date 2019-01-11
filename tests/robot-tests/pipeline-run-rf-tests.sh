#!/bin/bash

cd tests/robot-tests
pipenv install
pipenv run robot --outputdir test-results/ --xunit xunit --include HappyPath -v headless:1 -v url:https://educationstatisticstest.z6.web.core.windows.net/ -v browser:chrome -v timeout:5 -v implicit_wait:5 tests/
