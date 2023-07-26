## misc test utils 

A cli tool to automate the miscellaneous testing of EES.


### structure 
This tool consists of 4 areas.

* `index.ts` - Entrypoint file. Contains inquirer.js code responsible for asking users questions, getting input etc.

* `modules` - contains code responsible for calling various services. 

* `services` - contains code responsible for making requests to APIs and various utility functions

* `utils` - contains various utilities that the files in `services` use.


### Getting Started
* Ensure you have Node / NPM installed and are using at least Node V16
* Copy the `.env.example` file to `.env` (`cp .env.example .env`) and fill out with your own values.
* If you want to upload a subject or publish a new release place a file containing a data and a meta file named `archive.zip` in the root of this directory (`~/misc-test-utils`)
* Install dependencies: `npm ci`
* Start the tool: `npm run start`
* Clean test data, node_modules etc.: `npm run clean`
* Generate new `.env` types: `npm run gen-env`
* Type check: `npm run tsc`
