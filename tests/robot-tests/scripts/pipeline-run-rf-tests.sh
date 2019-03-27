#!/bin/bash

robot_env=$1

if [ "${robot_env}" != "test" && "${robot_env}" != "stage" && "${robot_env}" != "prod" ]
then
    robot_env="test"
fi

pipenv install
pipenv run python run_tests.py --ci -e ${robot_env}
