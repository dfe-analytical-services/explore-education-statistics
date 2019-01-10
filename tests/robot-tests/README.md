# What is this?

This was created in order to help testers get started quickly on a new project, and give new testers an idea of how to approach a new project. It gives you several things:

* A test runner script
   * Headless testing
   * Profiling
   * Happypath
   * Running test suites in parallel
* Useful RF keywords
   * Example python library of keywords
   * Example robot framework library of keywords

# What do I need to install

* Python3 (which should include pip3)

Then install pipenv
```
pip3 install pipenv
```

Then in the test directory run

```
pipenv install
```

# How do I run the tests?

```
pipenv run ./run\_tests.py
```

OR

```
pipenv shell
./run\_tests.py
```

Further instructions on how to use the test runner are included as a comment inside the run\_tests.py file itself.

# Who should I talk to?

Mark Youngman
