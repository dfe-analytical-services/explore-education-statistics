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

## Running the tests

Firstly, start InfluxDB and Grafana:

```bash
cd tests/performance-tests
docker-compose up influxdb grafana # use docker-compose -d instead if you want to run in daemon mode
```

Then run individual tests:

```bash
cd tests/performance-tests
docker-compose run k6 run src/path/to/test/script
```

An example of running an actual script would be:

```bash
docker-compose run k6 run src/local/latest-release/absence.test.js
```

The `src` directory contains tests based on environments (i.e. `dev` contains tests specific to the dev environment etc.)

## Transpiling and Bundling

By default, k6 can only run ES5.1 JavaScript code. To use TypeScript, we have to set up a bundler that converts TypeScript to JavaScript code.

Due to the fact that TypeScript doesn't provide a lot of value, we've chosen to not use Typescript and use JS instead.

If you want to learn more, check out [Bundling node modules in k6](https://k6.io/docs/using-k6/modules#bundling-node-modules).