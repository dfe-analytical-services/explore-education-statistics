# What is this?

This test framework runs UI tests against the Explore Education Statistics service using selenium and robot framework.

Currently, these tests are being maintained so they can be run on Linux or Windows. They're not being maintained for MacOS, but they may work without too much trouble. If you try MacOS, ensure you're using python3 and not python2!

# What do I need to install?

Firstly, install python3
   * For Windows, you'll need to download python3 from here: https://www.python.org/downloads/
   * For Linux, use the package manager (i.e. "sudo apt-get install python3")

Then ensure python and pip are included in your PATH environment variable
   * You can check whether this is necessary by entering `python3` and `pip3` into a terminal. The commands should be recognised.
   * For Windows, I needed to add `C:\Program Files\Python36` and `C:\Program Files\Python36\Scripts`. Check your Program Files directory to find out where python is installed on your computer. A search engine will tell you how to add them to the PATH. 
   
Then install pipenv
```
pip3 install pipenv
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
This holds chromedriver, used by selenium to interact with the browser. If chromedriver isn't present in this directory, it is automatically downloaded when the tests are run. If you're having trouble (e.g. browser and chromedriver versions aren't compatible) you can also manually place your chromedriver of choice in this directory.

# Guidelines for people writing UI tests

It is essential that we ensure test suites can run in parallel. This might not be the case if one test suite relies on test data that another changes. AND this might cause the tests, when run in parallel, to fail in unpredictable ways, making it difficult to determine what test data is the failure-making culprit.

For this reason, you **MUST** ensure that if you test suite requires test data that will change, that you create test new data. Be careful if scavenging test data from other test suites! Ideally, every test suite will use test data that is only used by those particular tests.

# Who should I talk to?

Mark Youngman
