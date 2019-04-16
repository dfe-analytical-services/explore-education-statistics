# What is this?

This test framework runs acceptance tests against the Explore Education Statistics service using selenium and robot framework.

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

You will also need to create a .env file which contains the basic auth username and password. You can copy the .env.example file in the robot-tests directory, replacing `user` and `pass` with the actual username and password.

# How do I run the tests?

```
pipenv run python run_tests.py
```

Further instructions on how to use the test runner are included inside the run\_tests.py file itself.

# Directory structure

This section details what the various directories in robot-tests contain.

### scripts
This directory holds scripts used by the run\_tests.py and the Azure pipeline.

### test-results
This directory holds the output of a test run, including the test report and log.

### tests
This holds the actual robot framework/selenium tests. The tests are themselves organised into different folders. The `libs` folder holds python and robot framework libraries used by the tests themselves.

### webdriver
This holds chromedriver, used by selenium to interact with the browser. It is automatically downloaded when the tests are run.

# Guidelines for people writing tests

It is essential that we ensure test suites can run in parallel. This might not be the case if one test suite relies on test data that another changes. AND this might cause the tests, when run in parallel, to fail in unpredictable ways, making it difficult to determine what test data is the failure-making culprit.

For this reason, you **MUST** ensure that if you test suite requires test data that will change, that you create test new data. Be careful if scavenging test data from other test suites! Ideally, every test suite will use test data that is only used by those particular tests.

# Who should I talk to?

Mark Youngman
