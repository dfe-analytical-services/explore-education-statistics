# What is this?

This test framework runs UI tests against the Explore Education Statistics service using selenium and robot framework.

Currently, these tests are being maintained so they can be run on Linux. They're not being maintained for Windows or MacOS, but they may work without too much trouble. If you try MacOS, ensure you're using python3 and not python2!

# What do I need to install?

Firstly, install python3.7 or greater
   * For Windows, you'll need to download python3 from here: https://www.python.org/downloads/
   * For Linux, use the package manager (i.e. On ubuntu, "sudo apt-get install python3.7")

Then ensure python and pip are included in your PATH environment variable
   * `python --version` should return a version >= 3.7. If it doesn't you can try using the commands `python3` or `python3.7`, if you have multiple versions of python installed on your machine.
   * To verify pip is installed, it is probably easiest to run entering `python -m pip` into a terminal. You should see the pip help text in response.
   * For Windows, I needed to add `C:\Program Files\Python37` and `C:\Program Files\Python37\Scripts`. Check your Program Files directory to find out where python is installed on your computer. A search engine will tell you how to add them to the PATH.
   
Then install pipenv
```
pip install pipenv
```

NOTE: If the above command doesn't work (or any of the subsequent commands in this README) then you can use `python -m` as below:
```
python -m pip install pipenv
```

Then in the robot-tests directory run
```
pipenv install
```

If you intend to run the tests from your local machine, you will also need to create .env files for the relevant environments: ".env.dev", ".env.dev02", and ".env.dev03". Each .env file contains the URLs for that environment's public app and admin app. You can copy and rename the .env.example file in the robot-tests directory, replacing `publicAppUrl` and `adminAppUrl` with the actual URLs for that environment.

When running these tests as part of the CI pipeline, they rely on `publicAppUrl` and `adminAppUrl` being set as part of the pipeline.

# How do I run the tests?

```
pipenv run python run_tests.py
```

Further instructions on how to use the test runner script
```
pipenv run python run_tests.py -h
```

# Directory structure

This section details what the various directories in robot-tests contain.

### scripts
This directory holds scripts used by run\_tests.py and the CI pipeline.

### test-results
This directory holds the output of a test run, including the test report and log.

### tests
This holds the actual robot framework/selenium tests. The tests are themselves organised into different folders. The `libs` folder holds python and robot framework libraries used by the tests themselves.

### webdriver
This holds chromedriver, used by selenium to interact with the browser. If chromedriver isn't present in this directory, it is automatically downloaded when the tests are run. You can explicitly download the chromedriver version of your choice with "--chromedriver <version>". Alternatively, you can place the chromedriver of your choice into the webdriver directory.

The run_tests.py defaults to using the chromedriver existing in the webdriver directory. If you wish the run script to download a different version, you'll first have to delete the preexisting chromedriver.

# Guidelines for people writing UI tests

### IDE
If searching for an IDE to add/edit these tests, consider using IntelliJ with "IntelliBot @ SeleniumLibrary Patched" and "Robot Framework Support" plugins. This should give you autocompletion and allow you to click through to keywords defined in both .robot and .py files. For this to work, you'll need to change the Project Structure to use "No SDK".

### Parallelism / Pabot
It is essential that the test suites can run in parallel. This might not be the case if one test suite relies on test data that another changes. AND this might cause the tests, when run in parallel, to fail in unpredictable ways, making it difficult to determine what test data is the failure-making culprit.

For this reason, you **MUST** ensure that if you test suite requires test data that will change, that you create test new data. Be careful if scavenging test data from other test suites! Ideally, every test suite will use test data that is only used by those particular tests.

# Who should I talk to?

Mark Youngman
