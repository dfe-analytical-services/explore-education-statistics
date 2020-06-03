[What is this?](#user-content-what-is-this)

[What do I need to install?](#user-content-what-do-i-need-to-install)

[How do I run the tests?](#user-content-how-do-i-run-the-tests)

[Running tests on the pipeline](#user-content-running-tests-on-the-pipeline)

[Webdriver](#webdriver)

[Authentication](#authentication)

[How do I backup and restore the test data on my local environment?](#user-content-how-do-i-backup-and-restore-the-test-data-on-my-local-environment)

[Directory structure](#user-content-directory-structure)

[Guidelines for people writing UI tests](#user-content-guidelines-for-people-writing-ui-tests)

[Who should I talk to?](#user-content-who-should-i-talk-to)  

# What is this?

This test framework runs UI tests against the Explore Education Statistics service using Selenium and Robot Framework.

Currently, these tests are being maintained so they can be run on Windows. They're not being maintained for Linux or MacOS, but they may work without too much trouble. If you try MacOS, ensure you're using Python3 and not Python2!

# What do I need to install?

Firstly, install Python3.7 or greater
   * For Windows, you'll need to download Python3 from here: https://www.python.org/downloads/
   * For Linux, use the package manager (i.e. On Ubuntu, "sudo apt-get install python3.7")

Then ensure python and pip are included in your PATH environment variable
   * `python --version` should return a version >= 3.7. If it doesn't you can try using the commands `python3` or `python3.7`, if you have multiple versions of python installed on your machine.
   * To verify pip is installed, it is probably easiest to run entering `python -m pip` into a terminal. You should see the pip help text in response.
   * For Windows, I needed to add `C:\Program Files\Python37` and `C:\Program Files\Python37\Scripts` to the PATH system variable. Check your Program Files directory to find out where Python is installed on your computer. A search engine will tell you how to add them to the PATH.
   
Then install pipenv
```
pip install pipenv
```

NOTE: If the above command doesn't work (or any of the subsequent commands in this README) then you can prefix `python -m ` to the command as below:
```
python -m pip install pipenv
```

Then in the robot-tests directory run
```
pipenv install
```
OR
```
python -m pipenv install
```

If you intend to run the tests from your local machine, you will also need to create .env files for the relevant environments: ".env.local", ".env.dev", ".env.test", ".env.preprod", and ".env.prod". You can copy and rename the .env.example file in the robot-tests directory, replacing the variable values with those for that file's specific environment. The tests rely on these environment variables being set.


# How do I run the tests?

```
pipenv run python run_tests.py
```

Further instructions on available options
```
pipenv run python run_tests.py -h
```


# Running tests on the pipeline

After changing the tests and ensuring they pass locally, it is then recommended that you run your changes on the pipeline to ensure they also pass there. There is a build pipeline you can use to check any changes you've made will pass on the pipeline: 'UI Tests'. You can run this pipeline using the branch with your changes, and it will produce an artifact you can download with the test report, screenshots, etc.

At the time of writing, the pipeline runs the tests against the Dev environment, and runs all general_public and admin tests. You can edit the pipeline if you wish to do something different.


# Authentication

To run the admin tests, the run_tests.py script uses ../../useful-scripts/auth-tokens/get_auth_tokens.py. The get_identity_info function logs in as a user and then returns the relevant local storage and cookies for the authenticated user. This is done so that if an authenticated user is required, a test run only needs to log in once rather than once for each test suite.

After the user has been logged in by the run_tests.py script, the local storage and cookie data for the authenticated user is saved in the IDENTITY_LOCAL_STORAGE.txt and IDENTITY_COOKIE.txt files. This is done so that if you're running the tests locally, you don't need to authenticate every time you rerun the tests, as the run_tests.py script will use the data they contain if they exist.


# Webdriver
The `webdriver` directory holds chromedriver, used by selenium to interact with the browser. If chromedriver isn't present in this directory, it is automatically downloaded when the tests are run. You can explicitly download the chromedriver version of your choice with "--chromedriver <version>". Alternatively, you can manually place the chromedriver of your choice into the webdriver directory.

NOTE: The run_tests.py script only downloads chromedriver if it doesn't already exist. If you wish the run script to download a different version, you'll first have to delete chromedriver from the webdriver directory.

If you need to change the chromedriver version used by the CI pipeline, it can be done in `scripts/pipeline-run-rf-tests.sh`. You can check [this repository](https://github.com/actions/virtual-environments/tree/master/images) for the version of chrome used on the Azure agent you're using. At the time of writing, the robot tests use the [Ubuntu 1604 image](https://github.com/actions/virtual-environments/blob/master/images/linux/Ubuntu1604-README.md)


# How do I backup and restore the test data on my local environment?

By following the instructions in this section, you can backup and restore all the data from your local environment, including content and statistics databases and all important Azure storage blobs and table.

You can currently only backup and restore data on your local environment on Windows. This is because currently you can only emulate Azure storage tables on Windows. At the time of writing [Azurite V3 only has support for blobs and queues, not tables](https://github.com/Azure/Azurite).

For the backup and restore scripts to work, you'll need:

- to be running the MsSQL database. You can do this using a docker container -- as per explore-education-statistics/src/docker-compose.yml -- from the src directory, `docker-compose up db`
- to be running AzureStorageEmulator
- to have AzCopy v7.3 installed ('C:\Program Files (x86)\Microsoft SDKs\Azure\AzCopy\AzCopy.exe' -- you can change where in the backup and restore scripts if you've installed it elsewhere)
- optionally, you might want Azure Data Studio and Azure Storage Explorer to inspect the MsSQL databases and your emulated blob, queue, and table storage.

To use the scripts, you'll need to install the dev dependencies:
```
pipenv install --dev
```

To backup:
```
pipenv run python backup-local.py
```

To restore:
```
pipenv run python restore-local.py
```

You can find the latest backup of the test data on Google Drive -- ask if you need a link.

The scripts deal with the content and statistics databases, the cache, downloads, and releases blob containers, and the imports storage table.

NOTES: 
- Be warned that the backup script will delete any files that were previously in your backup-data directory!
- Be warned that the restore script will cause you to lose the data in your local environment!
- Before you run the backup-local.py script, you may need to put a message into your local `generate-all-content` queue to regenerate the content cache. If you don't, your backup of the cache will be out of sync with the database backup!
- There is currently an issue where restoring data to the SQL DB means that services can no longer log in.


# Directory structure

This section details what the various directories in robot-tests contain.

### backup-data
This directory holds backup data for both the MsSQL databases, and the emulated storage blobs and tables. If you run backup-local.py, the backup is stored here. If you run restore-local.py, it uses the data in this directory to restore to your docker databases and emulated Azure storage.

### scripts
This directory holds scripts used by run\_tests.py and the CI pipeline.

### test-results
This directory holds the output of a test run, including the test report and log. Screenshots are preserved across runs, but other files are overwritten.

### tests
This holds the actual robot framework/selenium tests. The tests are themselves organised into different folders. The `libs` doesn't contain tests, but utility keywords used by the tests.

### webdriver
This holds chromedriver, used by selenium to interact with the browser.


# Guidelines for people writing UI tests

### Parallelism / Pabot
It is essential that the test suites can run in parallel. This might not be the case if one test suite relies on test data that another changes. AND this might cause the tests, when run in parallel, to fail in unpredictable ways, making it difficult to determine what test data is the failure-making culprit.

For this reason, you **MUST** ensure that if a test suite changes test data, that you create test new data to be used specifically with that suite. Be careful if scavenging test data from other test suites! Ideally, every test suite will use test data that is only used by those particular tests.

### Test data
After a group discussion, it was decided that tests will, as far as is possible, create their own test data. This means that any tests you write that alter data, you will need to create that data from scratch.

After an environment's data has been reset and reseeded, you will need to create some test data in the content database for the UI tests to work:
* Themes - "Test theme" with the Id `449d720f-9a87-4895-91fe-70972d1bdc04`
* Methodologies - "Test methodology" with the Id `b4886b45-53c2-4d6f-e6bc-08d77d76f342`, and Status `Approved`

### IDE
If searching for an IDE to add/edit these tests, consider using IntelliJ with "IntelliBot @ SeleniumLibrary Patched" and "Robot Framework Support" plugins. This should give you autocompletion and allow you to click through to keywords defined in both .robot and .py files. For this to work, you'll need to change the Project Structure to use "No SDK".

IntelliJ also allows you use to External Tools to right click on a file and run that file exclusively with these settings:
- Program: `/home/${USER}/.local/bin/pipenv` OR `C:\Python37\Scripts\pipenv.exe` OR wherever pipenv is located -- use `whereis pipenv` on linux or `where pipenv` on windows
- Arguments: `run python run_tests.py --visual -i robot -f "$FilePath$" -e dev`
- Working directory: `$ProjectFileDir$` (which should represent the robot-tests directory. You may have to do something like `$ProjectFileDir$/../tests/robot-tests`)


# Who should I talk to?

Mark Youngman
