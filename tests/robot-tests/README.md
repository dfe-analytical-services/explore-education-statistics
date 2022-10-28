# Explore Education Statistics Robot Framework tests

- [Explore Education Statistics Robot Framework tests](#explore-education-statistics-robot-framework-tests)
  - [**What is this?**](#what-is-this)
  - [**What do I need to install?**](#what-do-i-need-to-install)
  - [**Code style**](#code-style)
  - [**How do I run the tests?**](#how-do-i-run-the-tests)
  - [**Running tests on the pipeline**](#running-tests-on-the-pipeline)
  - [**Authentication**](#authentication)
  - [**Webdriver**](#webdriver)
  - [**Directory structure**](#directory-structure)
  - [**scripts**](#scripts)
  - [**test-results**](#test-results)
  - [**tests**](#tests)
  - [**webdriver**](#webdriver-1)
  - [**Snapshots**](#snapshots)
  - [**Guidelines for people writing UI tests**](#guidelines-for-people-writing-ui-tests)
  - [**Parallelism / Pabot**](#parallelism--pabot)
  - [**Test data:**](#test-data)
  - [**IDE**](#ide)
    - [**Additional IntelliJ settings**](#additional-intellij-settings)
  - [**Troubleshooting**](#troubleshooting)
    - [The tests are flaky when I run them locally.](#the-tests-are-flaky-when-i-run-them-locally)
    - [Test fails after not finding an element after x amount of seconds.](#test-fails-after-not-finding-an-element-after-x-amount-of-seconds)
    - [I get the following error when trying to run a UI test](#i-get-the-following-error-when-trying-to-run-a-ui-test)
    - [When I run the test, it automatically downloads chromedriver for ARM?](#when-i-run-the-test-it-automatically-downloads-chromedriver-for-arm)
- [**Who should I talk to?**](#who-should-i-talk-to)


## **What is this?**
This test framework runs UI tests against the Explore Education Statistics service using Selenium and Robot Framework.

Currently, these tests are being maintained so they can be run on Windows. They're not being maintained for Linux or MacOS, but they may work without too much trouble. 

## **What do I need to install?**

Firstly, install Python 3.10 or greater
   * For Windows, you'll need to download Python3 from here: https://www.python.org/downloads/
   * For Linux, use the package manager (i.e. On Ubuntu, `sudo apt-get install python3.10`)
   * For Mac, use the package manager: `brew install python@3.10`. You then need to change the default python symlink to the version you've just installed `ln -s -f /usr/local/bin/python3.10 /usr/local/bin/python` 

Then ensure python and pip are included in your PATH environment variable
   * `python --version` should return a version >= 3.10. If it doesn't you can try using the commands `python3` or `python3.10`, if you have multiple versions of python installed on your machine.
   * To verify `pip` is installed, it is probably easiest to run entering `python -m pip` into a terminal. You should see the pip help text in response.
   * For Windows, I needed to add `C:\Program Files\Python38` and `C:\Program Files\Python38\Scripts` to the `PATH` system variable. Check your Program Files directory to find out where Python is installed on your computer. A search engine will tell you how to add them to the `PATH`.
   
Then install `pipenv`:

```
pip install pipenv
```

NOTE: If the above command doesn't work (or any of the subsequent commands in this README) then you can prefix `python -m ` to the command as below:
```
python -m pip install pipenv
```

From the project root run:

```
pipenv install
```
OR
```
python -m pipenv install
```

If you intend to run the tests from your local machine, you will also need to create `.env` files for the relevant environments: 

- `.env.local`
- `.env.dev`
- `.env.test`
- `.env.preprod`
- `.env.prod`
 
You can copy and rename the `.env.example` file in the `robot-tests` directory, replacing the variable values with those for that file's specific environment. The tests rely on these environment variables being set.

Variables you may want to set in a `.env` file are documented in `.env.example`.

## **Code style**

In order to adhear to various linting & formatting rules, we use a few formatting and static-analysis tools to keep both Python & RobotFramework code clean. These are as follows:

* [Flake8](https://pypi.org/project/flake8/): 
We use Flake8 to verify pep8, pyflakes & circular complexity rules. 

* [Black](https://pypi.org/project/black/): 
We use black to adhear to PEP8 and [the black code style](https://black.readthedocs.io/en/stable/the_black_code_style/current_style.html)

* [Isort](https://pypi.org/project/isort/):
We use Isort to organise imports

* [Robotframework-tidy](https://pypi.org/project/robotframework-tidy/):
We use RobotFramework-tidy to format robotframework test code

## **How do I run the tests?**

From the `tests/robot-tests` directory run:

```
pipenv run python run_tests.py
```

This script is responsible for running the UI tests. All available options for `run_tests.py` can be seen by running `pipenv run python run_tests.py --help`

Here is an example:

- `pipenv run python run_tests.py -f tests/admin/bau/ -i robot -b chrome -e dev`

## **Running tests on the pipeline**

After changing the tests and ensuring they pass locally, it is then recommended that you also run your changes against the Dev environment. This can be done by either running the tests locally against dev (`run_tests.py -e dev`) or by utilising the Azure DevOps Build pipeline "UI Tests".

You can run the "UI Tests" pipeline using the branch with your changes, and it will produce an artifact you can download with the test report, screenshots, etc.

If you wish to run tests against other environments, it's recommended to do so from your own machine. Intermittent failures that only happen on the pipeline are rare with non-dev environments, as only the general_public tests are run against them, so for now this has sufficed.

Tests are occasionally tagged to not run against a given environment (you may see tests tagged with `NotAgainstDev`, `NotAgainstPreProd` etc. if that is the case). This usually happens to test data differences throughout environments.

## **Authentication**

To run the admin tests, the `run_tests.py` script uses `scripts/get_auth_tokens.py`. The `get_identity_info` function logs in as a user and then returns the relevant local storage and cookies for the authenticated user. This is done so that if an authenticated user is required, a test run only needs to log in once rather than once for each test suite.

After the user has been logged in by the `run_tests.py` script, the local storage and cookie data for the authenticated user is saved in the `IDENTITY_LOCAL_STORAGE_{USER}.txt` and `IDENTITY_COOKIE_{USER}.txt` files. This is done so that if you're running the tests locally, you don't need to authenticate every time you rerun the tests, as the run_tests.py script will use the data they contain if they exist.

## **Webdriver**

The `webdriver` directory holds `chromedriver`, used by selenium to interact with the browser. If `chromedriver` isn't present in this directory, it is automatically downloaded when the tests are run. You can explicitly download the `chromedriver` version of your choice with `--chromedriver <version>`. Alternatively, you can manually place the `chromedriver` of your choice into the `webdriver` directory. 

NOTE: The `run_tests.py` script only downloads `chromedriver` if it doesn't already exist. If you wish the run script to download a different version, you'll first have to delete `chromedriver` from the `webdriver` directory.

`run_tests.py` currently downloads the latest chromedriver by default. You can select a specific version by deleteing your webdriver directory and using the script's `--chromedriver` argument. This will download the chromedrive version you've chosen.

The latest version of chromedriver should always work for the pipeline. But if you need a specific version for the CI pipeline, you will need to add the `--chromedriver` argument to the `run_tests.py` call inside `scripts/pipeline-run-rf-tests.sh`. You can check [this repository](https://github.com/actions/virtual-environments/tree/master/images) for the version of chrome used on the Azure agent you're using. At the time of writing, the robot tests use the [Ubuntu 2004 image](https://github.com/actions/virtual-environments/blob/master/images/linux/Ubuntu2004-README.md)


## **Directory structure**
This section details what the various directories in robot-tests contain.

## scripts
This directory holds scripts used by `run_tests.py` and the CI pipeline.

## test-results
This directory holds the output of a test run, including the test report and log. Screenshots are preserved across runs, but other files are overwritten.

## tests
This holds the actual robot framework/selenium tests. The tests are themselves organised into different folders. The `libs` doesn't contain tests, but utility keywords used by the tests. Similar to `libs`, `files` contains files used by the tests.

## webdriver
This holds chromedriver, used by selenium to interact with the browser.

## **Snapshots**
To monitor changes to pages on Production, we use snapshots that are stored in `robot-tests/tests/snapshots`. These snapshots are created using the `create_snapshots.py` script in the `robot-tests/scripts` directory. 

You can refresh the current snapshots by running:

```
cd robot-tests
pipenv run python scripts/create_snapshots.py
```

These snapshot files are used by the test suite `tests/general_public/check_snapshots.robot`. If the snapshot doesn't match the current page, the test case fails, and an alert is sent to Slack.


## **Guidelines for people writing UI tests**

## Parallelism / Pabot
It is essential that the test suites can run in parallel. This might not be the case if one test suite relies on test data that another changes. This might cause the tests, when run in parallel, to fail in unpredictable ways, making it difficult to determine what test data is the failure-making culprit.

For this reason, you **MUST** ensure that if a test suite changes test data, that you create test new data to be used specifically with that suite. Be careful if scavenging test data from other test suites! Ideally, every test suite will use test data that is only used by those particular tests.

A common error is for a test suite to intermittently fail when run with other test suites, as it was previous only run by itself. Your tests must pass even if other publications and releases exist within the same topic, when multiple test suites are run at once.

## Test data:

After a group discussion, it was decided that tests will, as far as is possible, create their own test data. This means that any tests you write that alter data, you will need to create that data from scratch.

This however isn't the case for some tests. Certain tests rely on other utilities (written in robot) that are responsible for bootstrapping a given environment with test data. Test suites to create the bootstrapped data can be found in `tests/bootstrap_data`.

## **IDE**

If searching for an IDE to add/edit these tests, consider using IntelliJ, Pycharm or VScode with the following extensions: 

IntelliJ / Pycharm: 
- [IntelliBot](https://plugins.jetbrains.com/plugin/7386-intellibot) 
- [Robot Plugin](https://plugins.jetbrains.com/plugin/7430-robot-plugin)

VScode:
- [Robot Framework Intellisense](https://marketplace.visualstudio.com/items?itemName=TomiTurtiainen.rf-intellisense)

### **Additional IntelliJ settings**
  This should give you autocompletion and allow you to click through to keywords defined in both `.robot` and `.py` files. For this to work in, you'll need to change the Project Structure to use "No SDK".

  IntelliJ also allows you use to External Tools to right click on a file and run that file exclusively with these settings:
  - Program: `/home/${USER}/.local/bin/pipenv` OR `C:\Python38\Scripts\pipenv.exe` OR wherever pipenv is located -- use `whereis pipenv` on linux or `where pipenv` on windows
  - Arguments: `run python run_tests.py --visual -i robot -f "$FilePath$" -e dev`
  - Working directory: `$ProjectFileDir$` (which should represent the robot-tests directory. You may have to do something like `$ProjectFileDir$/../tests/robot-tests`)

## Visual testing - tables, charts and permalinks

We provide a suite of tests for visually checking tables, charts and permalinks and comparing before and after images. More information can be found in the [Visual Testing README](scripts/visual-testing/README.md).

## **Troubleshooting**

### The tests are flaky when I run them locally.

Try running the frontend with `npm run build & npm run start:prod`.

### Test fails after not finding an element after x amount of seconds.

Try using the following command to run the tests: 

You can also increase the length of waits via arguments to `run_tests.py`. You can see the options available by running `pipenv run python run_tests.py --help`.

To find the specific keyword that needs an increased wait, you can see what failed in the logs in `test-results` or utilise `run_tests.py`'s `--print-keywords` argument.

You can also increase the length of waits via environment variables. See the variables listed in `.env.example`. Change the relevant `.env.*` file when running tests locally, or by changing Azure DevOps variables for the CI pipeline.

### I get the following error when trying to run a UI test 

```
    raise Exception(f"Timeout! Couldn't find element with xpath selector '{selector}'")
Exception: Timeout! Couldn't find element with xpath selector '//div[text()="Stay signed in?"]'

During handling of the above exception, another exception occurred:

Traceback (most recent call last):
  File "run_tests.py", line 253, in <module>
    setup_authentication()
  File "run_tests.py", line 234, in setup_authentication
    setup_auth_variables(
  File "C:\Users\Hive\explore-education-statistics\tests\robot-tests\tests\libs\setup_auth_variables.py", line 59, in setup_auth_variables
    os.environ[local_storage_name], os.environ[cookie_name] = get_identity_info(
  File "C:\Users\Hive\explore-education-statistics\tests\robot-tests\scripts\get_auth_tokens.py", line 74, in get_identity_info
    raise AssertionError('Error when entering/submitting password')
AssertionError: Error when entering/submitting password
```
This error typically occurs when the BAU user password has expired and `setup_auth_variables` fails to login as the BAU user. You will need to update the password for the following users (as they usually expire around the same time).
* BAU 
* Analyst 
* Pre-release

### running-snapshot-tests
Refer to the `create_snapshots.py` script for more information (located in `robot-tests/scripts`).

### When I run the test, it automatically downloads chromedriver for ARM?

There is a known bug with pyderman which is responsible for downloading chromedriver whereby it defaults to downloading the ARM version of chrome if you're using an intel based mac. This means that if you are running the UI tests on an intel based mac the tests fail due to the wrong arch of Chrome being downloaded.

To fix this, you can manually download the correct [chromedriver](https://chromedriver.chromium.org/downloads) and put it in the webdriver directory.

# Who should I talk to?

Luke Howsam  
Mark Youngman
Duncan Watson