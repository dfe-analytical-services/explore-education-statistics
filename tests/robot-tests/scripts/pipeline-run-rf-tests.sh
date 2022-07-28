#!/bin/bash

# NOTE(mark): The slack webhook url, and admin and analyst passwords to access to Admin app are
# stored in the CI pipeline as secret variables, which means they cannot be accessed as normal
# environment variables, and instead must be passed as an argument to this script.

# WARNING: If any of the passwords contain a "$", bash will not pass the variable to run_tests.py correctly. 
# Our current solution to this is to escape any dollars where the password is stored 
# i.e. password "hello$world" should be stored as "hello\$world"

admin_pass=""
analyst_pass=""
slack_webhook_url=""
env=""
file=""

while [[ $# -gt 0 ]]; do
  key="$1"
  case "$key" in
    --admin-pass)
      shift
      admin_pass="$1"
      ;;
    --analyst-pass)
      shift
      analyst_pass="$1"
      ;;
    --slack-webhook-url)
      shift
      slack_webhook_url="$1"
      ;;
    --env)
      shift
      env="$1"
      ;;
    --file)
      shift
      file="$1"
  esac
  shift
done

[[ "$admin_pass" == "" ]] && { echo "Provide an admin password with an '--admin-pass PASS' argument"; exit 1; }
[[ "$analyst_pass" == "" ]] && { echo "Provide an analyst password with an '--analyst-pass PASS' argument"; exit 1; }
[[ "$env" == "" ]] && { echo "Provide an environment with an '--env ENV' argument"; exit 1; }
[[ "$file" == "" ]] && { echo "Provide a file/dir to run with an '--file FILE/DIR' argument"; exit 1; }

google-chrome-stable --version

python -m pip install --upgrade pip
pip install pipenv
pipenv install
pipenv run python run_tests.py --admin-pass "$admin_pass" --analyst-pass "$analyst_pass" --slack-webhook-url "$slack_webhook_url" -e $env --ci --file $file --processes 3 --enable-slack
