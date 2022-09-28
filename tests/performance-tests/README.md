# EES performance tests

The performance test suite is built using [k6](https://k6.io/) and visualised using 
[Grafana](https://grafana.com/) and [InfluxDB](https://www.influxdata.com/).

## How it works

* We run InfluxDB (for collecting data) and Grafana (for visualising the collected data) using docker-compose.
* We run the K6 tests using docker-compose.  We can choose specific tests to run from the CLI.
* The K6 tests run and post data to InfluxDB.
* Grafana is set up with a Dashboard that consumes and visualises data from the InfluxDB data source.

## Installation

### Requirements

You will need the following dependencies to run the tests successfully:

- [Docker and Docker Compose](https://docs.docker.com/)
- [NodeJS v16+](https://nodejs.org/)

## Running the tests

NOTE: All commands in this README are issued from the `tests/performance-tests` folder.

### Install dependencies with NPM

Run:

```bash
npm ci
```

### Add ees.local to your hosts file

This step is only necessary if running tests against the host machine. Add the following line to
your hosts file:

```
127.0.0.1    ees.local
```

### Start InfluxDB and Grafana

Note that this step isn't strictly necessary if we're not wanting to record metrics whilst
generating load - the tests can run independently of these if we're just wanting to generate
some load, for example.

```bash
npm start
```

To stop them later, run `npm stop`. Note that this will not destroy any data. To remove any data, run `npm run stop-and-clear-data`. 

### Compile the tests

The tests are written in Typescript so we need to transpile them to work in K6 (which as an aside is 
Go-based, not Node JS-based). We use Webpack and Babel for this.

```bash
npm run webpack # use "npm run webpack watch" instead if wanting to continue running webpack 
                # repeatedly during test development 
```

### Run the tests

#### Create environment-specific .env.<environment>.json files 

This step is only required if running performance tests that require access to the Admin API.
These files will store environment-specific user credentials for accessing Admin.

As a one-off, we will need to copy `tests/performance-tests/.env.example.json` to
`tests/performance-tests/.env.<environment>.json`, and supply the file with the correct
environment-specific credentials for the users that we'll be using.  The `<environment>` can
be any value that we can load test against. As an example, if doing load testing or test script
development against a local environment, we would create a file called
`tests/performance-tests/.env.local.json` and supply the local user credentials within the file.

#### Allow access to host ports from containers 

This step is only required if running performance tests locally against the host machine.

If running Ubuntu and running the tests against your local machine, ports under test from K6
will be protected by default by `Ubuntu Firewall`.

To grant access to these ports from containers on the 
[K6 subnet as defined in docker-compose.yml](tests/performance-tests/docker-compose.yml), 
run the following commands:

```bash
sudo ufw allow from 172.30.0.0/24 to any port 3000 # Public site
sudo ufw allow from 172.30.0.0/24 to any port 5000 # Data API
sudo ufw allow from 172.30.0.0/24 to any port 5010 # Content API
sudo ufw allow from 172.30.0.0/24 to any port 5021 # Admin site / Admin API
sudo ufw allow from 172.30.0.0/24 to any port 5030 # Keycloak
```

#### Obtain auth tokens for Admin testing

This step is only required if running performance tests that require access to the Admin API.
It requires the above step of creating environment-specific .env.json files first.

```bash
npm run store-environment-details --environment=<environment> --users=<user names>
``` 

This obtains an `access_token` and a `refresh_token` that can be used to access protected resources
in the Admin API.  The `refresh_token` allows long-running tests to refresh their access token if
it's going to expire mid-test.

This will look in the `.env.<environment>.json` file for a user with `"name": "<user name>"` and use 
that user's credentials to log into Admin in order to obtain their auth tokens.

Details of the environment and the users' access tokens can then be found in a generated file
named `dist/.environment-details.<environment>.json`. This file is then used by the tests 
themselves to run against the same environment.

As a concrete example:

```bash
npm run store-environment-details --environment=local --users=bau1
```

#### Run individual tests

```bash
npm run test dist/some-test-script.test.js --environment=<environment>
```

An example of running an actual script would be:

```bash
npm run test dist/import.test.js --environment=local
```

Each test script is runnable against any environment. They will find existing or set up new
dependent data before the test's VUs begin running, and will tear down any data that cannot be 
reused by a subsequent run when the tests finish. This is achieved using K6's `setup()` and 
`teardown()` lifecycles.

### Monitor the test results

View the results of real-time or historic test runs by visiting:

```
http://localhost:3005/d/ees-dashboard/ees-dashboard?orgId=1&refresh=5s
```

This Dashboard shows EES-specific custom metrics, such as the length of time it takes to 
complete table tool queries, the stages of progress that importing data files have reached
so far, and so on.

Also we can visit:

```
http://localhost:3005/d/k6/k6-load-testing-results?orgId=1&refresh=5s
```

This is an out-of-the-box Grafana / K6 Dashboard that captures general low-level performance
statistics.

## Transpiling and Bundling

The tests are written in Typescript so we need to transpile them to work in K6 (which as an aside is
Go-based, not Node JS-based). By default, k6 can only run ES5.1 JavaScript code. To use TypeScript, we
set up a bundler that converts TypeScript to JavaScript code.

If you want to learn more, check out 
[Bundling node modules in k6](https://k6.io/docs/using-k6/modules#bundling-node-modules).

We also have some Typescript Node scripts that are used directly by Node.js that require bundling and 
transpiling in the same fashion.

## Troubleshooting

### Exceptions and stacktraces in Typescript code

We can use source maps to be able to trace errors back to the original Typescript source when 
encountering errors in transpiled Javascript code.  This is provided via the `source-map-support` 
package.

To enable this in Node, we can supply the `-r source-map-support/register` option whilst running the 
problem script.  For example:

```bash
node -r source-map-support/register dist/logAuthTokens.js local bau1
```

would result in stacktraces that point back to the original `src/auth/logAuthTokens.ts` Typescript 
file.

### Healthcheck tests

We have some test scripts that ascertain that the infrastructure of the environment we're testing 
against and our own utils and helper scripts are behaving themselves.

#### Refresh token support

Run the following to test that the environment under test supports refresh tokens (as specified
in your `.env.<environment>.json file).

```bash
npm run store-environment-details --environment=<environment> --users=bau1)
npm run test dist/refreshTokens.test.js --environment=local
```

The output should look like:

```text
     ✓ response with original access token was 200
     ✓ response with refreshed tokens was successful
     ✓ response with refreshed tokens contained a new accessToken
     ✓ response with refreshed tokens contained a new refreshToken
     ✓ response with refreshed access token was 200
     ✓ response with re-refreshed tokens was successful
     ✓ response with re-refreshed tokens contained a new accessToken
     ✓ response with re-refreshed tokens contained a new refreshToken
     ✓ response with re-refreshed access token was 200
```
