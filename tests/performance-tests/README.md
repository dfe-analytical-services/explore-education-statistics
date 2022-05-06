<div align="center">
  

# EES performance tests 


</div>

## Prerequisites

- [k6](https://k6.io/docs/getting-started/installation)
- [NodeJS](https://nodejs.org/en/download/)
- [NPM](https://www.npmjs.com/)


**Install dependencies**


```bash
npm ci 
```

## Running the test

```bash
k6 run test.js
```

* use the `k6 run` command in order to run files. By default the tests will output to a html file (in the `test-results` directory).

* the `src` directory contains tests based on environments (i.e. `dev` contains tests specific to the dev environment etc.)

### Transpiling and Bundling

By default, k6 can only run ES5.1 JavaScript code. To use TypeScript, we have to set up a bundler that converts TypeScript to JavaScript code.

Due to the fact that TypeScript doesn't provide a lot of value, we've chosen to not use Typescript and use JS instead.

If you want to learn more, check out [Bundling node modules in k6](https://k6.io/docs/using-k6/modules#bundling-node-modules).
